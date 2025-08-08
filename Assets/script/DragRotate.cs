using UnityEngine;

public class DragRotate : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float limitAngle = 20f; // X,Y の最大回転角
    public float returnSpeed = 2f; // 中心に戻るスピード（大きいほど速い）

    private Vector2 lastPointerPos;
    private bool dragging = false;

    void Update()
    {
        // --- PC ---
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            lastPointerPos = (Vector2)Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }
        if (dragging && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastPointerPos;
            ApplyRotation(delta);
            lastPointerPos = (Vector2)Input.mousePosition;
        }

        // --- モバイル ---
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                lastPointerPos = t.position;
                dragging = true;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                Vector2 delta = t.position - lastPointerPos;
                ApplyRotation(delta);
                lastPointerPos = t.position;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                dragging = false;
            }
        }

        // --- ドラッグしていないときは中心へ戻す ---
        if (!dragging)
        {
            Vector3 e = transform.eulerAngles;
            e.z = 180f;

            e.x = NormalizeAngle(e.x);
            e.y = NormalizeAngle(e.y);

            e.x = Mathf.MoveTowards(e.x, 0f, returnSpeed * Time.deltaTime);
            e.y = Mathf.MoveTowards(e.y, 0f, returnSpeed * Time.deltaTime);

            transform.eulerAngles = e;
        }
    }

    private void ApplyRotation(Vector2 delta)
    {
        float rotX = delta.y * rotationSpeed * Time.deltaTime;
        float rotY = -delta.x * rotationSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up, rotY, Space.World);
        transform.Rotate(Vector3.right, rotX, Space.World);

        // --- Z軸を固定 ---
        Vector3 e = transform.eulerAngles;
        e.z = 180f;

        e.x = NormalizeAngle(e.x);
        e.y = NormalizeAngle(e.y);

        e.x = Mathf.Clamp(e.x, -limitAngle, limitAngle);
        e.y = Mathf.Clamp(e.y, -limitAngle, limitAngle);

        transform.eulerAngles = e;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
