using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public float slideForce = 10f;
    public float slideDuration = 0.4f;
    public float spikeForce = 18f;
    public float receiveForce = 12f;
    public float hitRadius = 1.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 initialScale;

    private float moveInput;
    public bool isGrounded;
    private bool isSliding;

    private bool jumpRequest;
    private bool spikeRequest;
    private bool slideRequest;
    private bool receiveRequest;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        initialScale = transform.localScale;
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!isSliding) Move();
        PerformPhysicsActions();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private void HandleInput()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.D)) moveInput += 1f;
        if (Input.GetKey(KeyCode.A)) moveInput -= 1f;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded && !isSliding) jumpRequest = true;
            else if (!isGrounded) spikeRequest = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (isGrounded && !isSliding) slideRequest = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isGrounded && !isSliding) receiveRequest = true;
        }
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * Mathf.Sign(moveInput), initialScale.y, initialScale.z);
        }
    }

    private void PerformPhysicsActions()
    {
        if (jumpRequest)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            jumpRequest = false;
        }

        if (slideRequest)
        {
            StartCoroutine(SlideRoutine());
            slideRequest = false;
        }

        if (spikeRequest)
        {
            if (anim != null) anim.SetTrigger("TriggerSpike");
            HitBall(spikeForce, new Vector2(Mathf.Sign(transform.localScale.x) * 0.8f, -1f).normalized);
            spikeRequest = false;
        }

        if (receiveRequest)
        {
            HitBall(receiveForce, new Vector2(Mathf.Sign(transform.localScale.x) * 0.3f, 1f).normalized);
            receiveRequest = false;
        }
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;

        if (anim != null)
        {
            anim.SetTrigger("TriggerPreslide");
            anim.SetTrigger("TriggerSlide");
        }

        rb.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.x) * slideForce, rb.linearVelocity.y);
        yield return new WaitForSeconds(slideDuration);
        isSliding = false;
    }

    private void UpdateAnimations()
    {
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
            anim.SetBool("IsJumping", !isGrounded);
        }
    }

    private void HitBall(float force, Vector2 direction)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Ball"))
            {
                Rigidbody2D ballRb = hit.GetComponent<Rigidbody2D>();
                if (ballRb != null)
                {
                    ballRb.linearVelocity = Vector2.zero;
                    ballRb.AddForce(direction * force, ForceMode2D.Impulse);
                }
            }
        }
    }
}