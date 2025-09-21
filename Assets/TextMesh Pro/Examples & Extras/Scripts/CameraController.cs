using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace TMPro.Examples
{
    public class CameraController : MonoBehaviour
    {
        public enum CameraModes { Follow, Isometric, Free }

        private Transform cameraTransform;
        private Transform dummyTarget;

        public Transform CameraTarget;

        public float FollowDistance = 30.0f;
        public float MaxFollowDistance = 100.0f;
        public float MinFollowDistance = 2.0f;

        public float ElevationAngle = 30.0f;
        public float MaxElevationAngle = 85.0f;
        public float MinElevationAngle = 0f;

        public float OrbitalAngle = 0f;

        public CameraModes CameraMode = CameraModes.Follow;

        public bool MovementSmoothing = true;
        public bool RotationSmoothing = false;
        private bool previousSmoothing;

        public float MovementSmoothingValue = 25f;
        public float RotationSmoothingValue = 5.0f;

        public float MoveSensitivity = 2.0f;

        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 desiredPosition;
        
        // Variaveis para o Novo Input System
        private Vector2 lookInput;
        private Vector2 panInput;
        private float zoomInput;
        private bool isLeftShiftPressed;

        void Awake()
        {
            if (QualitySettings.vSyncCount > 0)
                Application.targetFrameRate = 60;
            else
                Application.targetFrameRate = -1;

            cameraTransform = transform;
            previousSmoothing = MovementSmoothing;
        }


        // Use this for initialization
        void Start()
        {
            if (CameraTarget == null)
            {
                // If we don't have a target (assigned by the player, create a dummy in the center of the scene).
                dummyTarget = new GameObject("Camera Target").transform;
                CameraTarget = dummyTarget;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            // O código de input foi movido para os métodos de callback
            
            // Handle Mouse Wheel Input
            float zoomMultiplier = isLeftShiftPressed ? 10f : 1f;
            FollowDistance -= zoomInput * zoomMultiplier * 5.0f;
            FollowDistance = Mathf.Clamp(FollowDistance, MinFollowDistance, MaxFollowDistance);

            // Handle Camera Look
            if (lookInput.magnitude > 0.01f)
            {
                ElevationAngle -= lookInput.y * MoveSensitivity;
                ElevationAngle = Mathf.Clamp(ElevationAngle, MinElevationAngle, MaxElevationAngle);

                OrbitalAngle += lookInput.x * MoveSensitivity;
                if (OrbitalAngle > 360) OrbitalAngle -= 360;
                if (OrbitalAngle < 0) OrbitalAngle += 360;
            }

            // Handle Camera Pan
            if (panInput.magnitude > 0.01f)
            {
                 if (dummyTarget == null)
                 {
                    dummyTarget = new GameObject("Camera Target").transform;
                    dummyTarget.position = CameraTarget.position;
                    dummyTarget.rotation = CameraTarget.rotation;
                    CameraTarget = dummyTarget;
                    previousSmoothing = MovementSmoothing;
                    MovementSmoothing = false;
                 }
                 else if (dummyTarget != CameraTarget)
                 {
                    dummyTarget.position = CameraTarget.position;
                    dummyTarget.rotation = CameraTarget.rotation;
                    CameraTarget = dummyTarget;
                    previousSmoothing = MovementSmoothing;
                    MovementSmoothing = false;
                 }
                
                Vector3 moveVector = cameraTransform.TransformDirection(panInput.x, panInput.y, 0);
                dummyTarget.Translate(-moveVector, Space.World);
            }

            // Check if we still have a valid target
            if (CameraTarget != null)
            {
                if (CameraMode == CameraModes.Isometric)
                {
                    desiredPosition = CameraTarget.position + Quaternion.Euler(ElevationAngle, OrbitalAngle, 0f) * new Vector3(0, 0, -FollowDistance);
                }
                else if (CameraMode == CameraModes.Follow)
                {
                    desiredPosition = CameraTarget.position + CameraTarget.TransformDirection(Quaternion.Euler(ElevationAngle, OrbitalAngle, 0f) * (new Vector3(0, 0, -FollowDistance)));
                }
                else
                {
                    // Free Camera implementation
                }

                if (MovementSmoothing == true)
                {
                    // Using Smoothing
                    cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, desiredPosition, ref currentVelocity, MovementSmoothingValue * Time.fixedDeltaTime);
                }
                else
                {
                    // Not using Smoothing
                    cameraTransform.position = desiredPosition;
                }

                if (RotationSmoothing == true)
                    cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.LookRotation(CameraTarget.position - cameraTransform.position), RotationSmoothingValue * Time.deltaTime);
                else
                {
                    cameraTransform.LookAt(CameraTarget);
                }
            }
        }
        
        //---------------------------------------------------------
        // Novos métodos de callback para o Input System
        //---------------------------------------------------------
        
        public void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

        public void OnZoom(InputValue value)
        {
            zoomInput = value.Get<Vector2>().y; // O scroll do mouse é no eixo Y
        }
        
        public void OnPan(InputValue value)
        {
            panInput = value.Get<Vector2>();
        }

        public void OnLeftShift(InputValue value)
        {
            isLeftShiftPressed = value.isPressed;
        }

        public void OnChangeModeToIsometric()
        {
             CameraMode = CameraModes.Isometric;
        }

        public void OnChangeModeToFollow()
        {
            CameraMode = CameraModes.Follow;
        }
        
        public void OnToggleSmoothing()
        {
             MovementSmoothing = !MovementSmoothing;
        }

        public void OnSelectTarget(InputValue value)
        {
             if (Camera.main != null && value.isPressed)
             {
                 Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                 RaycastHit hit;
                 
                 if (Physics.Raycast(ray, out hit, 300, 1 << 10 | 1 << 11 | 1 << 12 | 1 << 14))
                 {
                     if (hit.transform == CameraTarget)
                     {
                         // Reset Follow Position
                         OrbitalAngle = 0;
                     }
                     else
                     {
                         CameraTarget = hit.transform;
                         OrbitalAngle = 0;
                         MovementSmoothing = previousSmoothing;
                     }
                 }
             }
        }
    }
}