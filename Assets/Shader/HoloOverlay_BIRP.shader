Shader "Custom/HoloOverlay_BIRP_Angle"
{
    Properties
    {
        _BaseColor          ("Base Color", Color) = (1,1,1,1)
        _FoilMask           ("Foil Mask (R)", 2D) = "white" {}
        _NormalMap          ("Normal Map", 2D)    = "bump" {}
        _RainbowLUT         ("Rainbow LUT", 2D)   = "white" {}

        _RainbowScale       ("Rainbow Scale", Float) = 1.2    // 傾き成分の縞密度
        _AngularScale       ("Angular Scale", Float) = 1.0    // 回転で虹が回る量
        _AngularBias        ("Angular Bias",  Float) = 0.0    // 位相微調整 0..1
        _IridescenceStrength("Iridescence Strength", Range(0,1)) = 0.8
        _EmissionBoost      ("Emission Boost", Range(0,2)) = 0.3
        _Opacity            ("Opacity", Range(0,1)) = 0.9
        _F0                 ("F0 (Fresnel Base)", Color) = (0.9,0.9,0.9,1)

        // 時間変化させたい場合だけ使用（0で無効）
        _RainbowSpeed       ("[Optional] Rainbow Speed (Time)", Float) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            fixed4 _BaseColor;
            fixed4 _F0;
            float  _RainbowScale, _AngularScale, _AngularBias, _IridescenceStrength, _EmissionBoost, _Opacity, _RainbowSpeed;

            sampler2D _FoilMask;   float4 _FoilMask_ST;
            sampler2D _NormalMap;  float4 _NormalMap_ST;
            sampler2D _RainbowLUT; float4 _RainbowLUT_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent: TANGENT;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float3 nWS : TEXCOORD1;
                float3 vWS : TEXCOORD2;
                float3 tWS : TEXCOORD3;
                float3 bWS : TEXCOORD4;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 n = UnityObjectToWorldNormal(v.normal);
                float3 t = normalize(mul((float3x3)unity_ObjectToWorld, v.tangent.xyz));
                float3 b = normalize(cross(n, t) * v.tangent.w);

                o.nWS = normalize(n);
                o.tWS = t;
                o.bWS = b;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 camPos   = _WorldSpaceCameraPos;
                o.vWS = normalize(camPos - worldPos);

                o.uv = v.uv;
                return o;
            }

            inline float3 FresnelSchlick(float ndv, float3 F0)
            {
                float f = pow(saturate(1.0 - ndv), 5.0);
                return F0 + (1.0 - F0) * f;
            }

            inline float3 SampleRainbowLUT(float h)
            {
                float2 uv = float2(frac(h), 0.5);
                return tex2D(_RainbowLUT, uv).rgb;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normal map（Unity標準）
                float3 nTS = UnpackNormal(tex2D(_NormalMap, TRANSFORM_TEX(i.uv, _NormalMap)));
                // TBN変換
                float3 N = normalize(nTS.x * i.tWS + nTS.y * i.bWS + nTS.z * i.nWS);

                float3 V = normalize(i.vWS);
                float  ndv = saturate(dot(N, V)); // 傾き成分

                // --- 角度（方位）成分：接線空間での視線ベクトルの方向 ---
                float3 Vt = float3( dot(V, normalize(i.tWS)),
                                    dot(V, normalize(i.bWS)),
                                    dot(V, normalize(i.nWS)) );

                // atan2 を 0..1 に正規化
                float angle01 = (atan2(Vt.y, Vt.x) / (2.0 * UNITY_PI)) + 0.5;

                // 虹の位相：回転（angle）＋傾き（ndv）＋任意で時間
                float h = angle01 * _AngularScale
                        + ndv     * _RainbowScale
                        + _AngularBias
                        + (_Time.y * _RainbowSpeed); // 使わなければ 0 に

                float3 rainbow = SampleRainbowLUT(h);

                // Fresnelで角度ブースト
                float3 F = FresnelSchlick(ndv, _F0.rgb);

                // マスク（白=有効）
                float mask = tex2D(_FoilMask, TRANSFORM_TEX(i.uv, _FoilMask)).r;

                // 合成
                float3 baseCol = _BaseColor.rgb;
                float3 holo = lerp(baseCol, rainbow, _IridescenceStrength);
                holo *= F;
                holo *= mask;

                float3 emission = rainbow * mask * _EmissionBoost;
                float  alpha    = mask * _Opacity;

                return fixed4(holo + emission, alpha);
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
