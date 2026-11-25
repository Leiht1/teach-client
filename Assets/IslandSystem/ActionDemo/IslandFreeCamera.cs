using UnityEngine;

namespace IslandSystem
{
    // Shares the island camera so sky, lighting and rendering settings stay consistent.
    [DefaultExecutionOrder(-100)]
    public sealed class IslandFreeCamera : MonoBehaviour
    {
        ActionMotor motor;
        ScratchWorkbenchHost workbench;
        Transform view;
        Vector3 position;
        float yaw, pitch;
        float speed = 24f;

        internal void Initialize(ActionMotor player, bool startFlying)
        {
            motor = player;
            view = player.view.transform;
            if (startFlying)
            {
                Enter(new Vector3(-220, 145, -265), Quaternion.LookRotation(new Vector3(220, -118, 265)));
            }
        }

        void Enter(Vector3 start, Quaternion rotation)
        {
            motor.FreeCameraActive = true;
            ActionMotor.ReleasePointer();
            position = start;
            yaw = rotation.eulerAngles.y;
            pitch = Mathf.DeltaAngle(0f, rotation.eulerAngles.x);
            view.SetPositionAndRotation(start, rotation);
        }

        void Update()
        {
            if (!motor || motor.ReviewMode) return;
            if (!workbench) workbench = FindAnyObjectByType<ScratchWorkbenchHost>();
            if (workbench && workbench.IsOpen) return;

            var chapter = FindAnyObjectByType<BeachChapter>();
            if (Input.GetKeyDown(KeyCode.F6) && (motor.FreeCameraActive || !chapter || !chapter.ManualLocked))
            {
                if (motor.FreeCameraActive)
                    motor.FreeCameraActive = false;
                else
                    Enter(view.position, view.rotation);
            }

            if (!motor.FreeCameraActive || !Application.isFocused) return;
            if (Input.GetMouseButton(1) && !Input.GetMouseButtonDown(1))
            {
                yaw += Input.GetAxisRaw("Mouse X") * 2.5f;
                pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * 2.5f, -89f, 89f);
            }

            speed = Mathf.Clamp(speed * Mathf.Pow(1.2f, Input.mouseScrollDelta.y), 4f, 120f);
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var horizontal = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            float elevation = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
            var direction = Vector3.ClampMagnitude(rotation * horizontal + Vector3.up * elevation, 1f);
            bool boost = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            position += direction * speed * (boost ? 4f : 1f) * Time.unscaledDeltaTime;
            view.SetPositionAndRotation(position, rotation);
        }
    }
}
