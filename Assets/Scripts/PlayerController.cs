using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(CharacterController))]
public class PlayerController_NewInput : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 0f;
    public float laneDistance = 2f;
    public float laneChangeSpeed = 8f;
    public float jumpForce = 10f;
    public float gravity = -20f;
    public float slideTime = 1f;

    [Header("Speed Increase")]
    public float speedIncreaseInterval = 10f;
    public float speedIncreaseAmount = 1f;

    private CharacterController cc;
    public int currentLane { get; private set; } = 1;

    private Vector3 moveVector;
    private float verticalVelocity = 0f;
    private bool isSliding = false;
    private Vector3 originalCenter;
    private float originalHeight;

    // swipe detection (EnhancedTouch)
    private Vector2 startTouch;
    private bool swipeDetected = false;
    private float minSwipeDistance = 50f;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        originalCenter = cc.center;
        originalHeight = cc.height;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        moveVector = Vector3.forward * forwardSpeed;

        float targetX = (currentLane - 1) * laneDistance;
        float diffX = targetX - transform.localPosition.x;
        float horizontal = Mathf.Clamp(diffX * laneChangeSpeed, -laneDistance * laneChangeSpeed, laneDistance * laneChangeSpeed);
        moveVector.x = horizontal;

        if (cc.isGrounded)
        {
            if (verticalVelocity < 0) verticalVelocity = -1f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        moveVector.y = verticalVelocity;

        cc.Move(moveVector * Time.deltaTime);

        HandleEnhancedTouchInput();

        HandleKeyboardFallback();
    }

    private void HandleEnhancedTouchInput()
    {
        if (Touch.activeTouches.Count > 0)
        {
            Touch t = Touch.activeTouches[0];

            if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                startTouch = t.screenPosition;
                swipeDetected = true;
            }
            else if ((t.phase == UnityEngine.InputSystem.TouchPhase.Moved || t.phase == UnityEngine.InputSystem.TouchPhase.Stationary) && swipeDetected)
            {
                Vector2 diff = t.screenPosition - startTouch;

                if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y) && Mathf.Abs(diff.x) > minSwipeDistance)
                {
                    if (diff.x > 0) MoveRight(); else MoveLeft();
                    swipeDetected = false;
                }
                else if (Mathf.Abs(diff.y) > minSwipeDistance)
                {
                    if (diff.y > 0) Jump(); else Slide();
                    swipeDetected = false;
                }
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended || t.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                swipeDetected = false;
            }
        }
        else
        {
            swipeDetected = false;
        }

        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                startTouch = mouse.position.ReadValue();
                swipeDetected = true;
            }
            else if (mouse.leftButton.wasReleasedThisFrame && swipeDetected)
            {
                Vector2 end = mouse.position.ReadValue();
                Vector2 diff = end - startTouch;
                if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y) && Mathf.Abs(diff.x) > minSwipeDistance)
                {
                    if (diff.x > 0) MoveRight(); else MoveLeft();
                }
                else if (Mathf.Abs(diff.y) > minSwipeDistance)
                {
                    if (diff.y > 0) Jump(); else Slide();
                }
                swipeDetected = false;
            }
        }
    }

    private void HandleKeyboardFallback()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) MoveLeft();
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) MoveRight();
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) Jump();
            if (Keyboard.current.downArrowKey.wasPressedThisFrame) Slide();
        }
    }

    public void MoveLeft()
    {
        if (isSliding) return;
        currentLane = Mathf.Clamp(currentLane - 1, 0, 2);
    }

    public void MoveRight()
    {
        if (isSliding) return;
        currentLane = Mathf.Clamp(currentLane + 1, 0, 2);
    }

    public void Jump()
    {
        if (cc.isGrounded && !isSliding)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    public void Slide()
    {
        if (!isSliding && cc.isGrounded)
        {
            StartCoroutine(DoSlide());
        }
    }

    private IEnumerator DoSlide()
    {
        isSliding = true;
        cc.height = originalHeight / 2f;
        cc.center = originalCenter - new Vector3(0, originalHeight / 4f, 0);
        yield return new WaitForSeconds(slideTime);
        cc.height = originalHeight;
        cc.center = originalCenter;
        isSliding = false;
    }

    // Função para resetar o jogador no início do jogo
    public void ResetPlayer()
    {
        // Garante que o jogador volte para o estado inicial
        transform.position = new Vector3(0f, 1f, 0f);
        forwardSpeed = 0f;
        currentLane = 1;
        verticalVelocity = 0f;
        
        // Se já houver uma corrotina de aumento de velocidade, pare-a para evitar conflitos
        StopAllCoroutines();
    }

    public IEnumerator IncreaseSpeedOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(speedIncreaseInterval);
            forwardSpeed += speedIncreaseAmount;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHit();
            }
        }
    }
}