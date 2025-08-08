using UnityEngine;

public class DragRotate : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float limitAngle = 20f; // X,Y �̍ő��]�p
    public float returnSpeed = 2f; // ���S�ɖ߂�X�s�[�h�i�傫���قǑ����j

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

        // --- ���o�C�� ---
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

        // --- �h���b�O���Ă��Ȃ��Ƃ��͒��S�֖߂� ---
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

        // --- Z�����Œ� ---
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
