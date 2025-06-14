using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject normalBackground;
    [SerializeField] private GameObject alternateBackground;

    //HillSwitch
    private bool inPathSwitchZone = false;
    private bool onHill = false;


    //Walk sound
    private float walkSoundToggleTimer = 0f;
    private bool playHighSound = true;

    public float speed = 8f;
    public float jumpForce = 30f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Animator animator;

    public Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;
    private float moveInput;

    private float horizontal;
    private bool isBeingPushed = false;
    private float platformSpeed;
    public bool playerOnPlatform = false;

    //Dashing
    private bool canDash = false;
    private bool isDashing;
    private float dashingPower = 5f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    // Dash Charge System
    private float maxDashCharge = 5f;
    private float currentDashCharge = 5f;
    private float dashRechargeRate = 1f;
    private float dashRechargeCooldownTimer = 0f;

    // Level Switching
    private bool isInAlternateLevel = false;
    [SerializeField] private Vector2 levelOffset = new Vector2(0f, -100f); // adjust this offset to match distance between levels
    private Vector2 originalLevelPosition;
    private Color originalBackgroundColor;
    private bool hasTeleported = false;
    private bool isFacingRight = true;

    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private TrailRenderer tr;

    //UI stuff
    public UI_Controls UI;

    [SerializeField] private AudioManager audioManager;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalBackgroundColor = Camera.main.backgroundColor;
        originalLevelPosition = transform.position;
    }

    void Update()
    {

        if (inPathSwitchZone)
        {
            if (Input.GetKeyDown(KeyCode.W) && !onHill)
            {
                SwitchToHillPath();
            }
            else if (Input.GetKeyDown(KeyCode.S) && onHill)
            {
                SwitchToGroundPath();
            }
        }


        // Dash Charge Recharge
        if (currentDashCharge < maxDashCharge)
        {
            currentDashCharge += dashRechargeRate * Time.deltaTime;
            currentDashCharge = Mathf.Min(currentDashCharge, maxDashCharge);
        }

        if (dashRechargeCooldownTimer > 0)
        {
            dashRechargeCooldownTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            return;
        }

        if (UI.pauseState == 0)
        {
            horizontal = Input.GetAxisRaw("Horizontal");

            // Walking sound logic
            if (horizontal != 0 && IsGrounded())
            {
                walkSoundToggleTimer -= Time.deltaTime;

                if (walkSoundToggleTimer <= 0f)
                {
                    // Stop all walking sounds before playing new one
                    audioManager.Stop("WalkHigh");
                    audioManager.Stop("WalkLow");
                    audioManager.Stop("MetalWalk");

                    if (isInAlternateLevel)
                    {
                        // Play looping metal walk sound in alternate world
                        audioManager.Play("MetalWalk");
                    }
                    else
                    {
                        // Play alternating footstep sounds
                        if (playHighSound)
                            audioManager.Play("WalkHigh");
                        else
                            audioManager.Play("WalkLow");

                        playHighSound = !playHighSound;
                    }

                    walkSoundToggleTimer = 0.5f;
                }
            }
            else
            {
                // Player is not moving or not grounded � stop walking sounds
                walkSoundToggleTimer = 0f;
                audioManager.Stop("WalkHigh");
                audioManager.Stop("WalkLow");
                audioManager.Stop("MetalWalk");
            }
        }

            // Ground check
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("IsJumping", !isGrounded); // Update jumping animation state

        // Movement
        if (!isBeingPushed)
        {
            moveInput = Input.GetAxis("Horizontal");
            horizontal = moveInput;
            rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
            animator.SetBool("IsWalking", moveInput != 0);
        }
        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && UI.pauseState == 0 && !isBeingPushed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            FindObjectOfType<AudioManager>().Play("Jump");
        }

        // Trigger dash when Shift key is pressed
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && currentDashCharge > 0)
        {
            if (canDash)
            {
                StartCoroutine(PortalDash());
                animator.Play("PlayerDash");
            }
        }


        // Flip sprite
        Flip();
    }

    private void FixedUpdate()
    {
        if (isDashing || isBeingPushed)
        {
            return;
        }

        if (!playerOnPlatform)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
        else if (horizontal != 0)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(platformSpeed, rb.velocity.y);
        }
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    public float GetHorizontal()
    {
        return horizontal;
    }

    public Rigidbody2D GetRB()
    {
        return rb;
    }
    public bool GetPushed()
    {
        return isBeingPushed;
    }

    public void SetPushed(bool pushed)
    {
        Debug.Log("pushed set to: " + pushed);
        isBeingPushed = pushed;
    }
    void Flip()
    {
        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;
    }

    private IEnumerator PortalDash()
    {
        // Reset teleport flag
        hasTeleported = false;

        canDash = false;
        isDashing = true;
        currentDashCharge -= 1f;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        tr.emitting = true;

        Vector2 dashDirection = isFacingRight ? Vector2.right : Vector2.left;
        float dashDistance = 5f;
        Vector2 entryPosition = (Vector2)transform.position + dashDirection * dashDistance;

        // Instantiate entry portal
        GameObject entryPortal = Instantiate(portalPrefab, entryPosition, Quaternion.identity);
        Animator portalAnimator = entryPortal.GetComponent<Animator>();
        portalAnimator.Play("Portal");

        // Set the player reference on the portal
        PortalTrigger portalTrigger = entryPortal.GetComponent<PortalTrigger>();
        if (portalTrigger != null)
        {
            portalTrigger.SetPlayerReference(this);
        }

        // Wait for portal animation to be ready
        float portalAnimationTime = 1f;
        yield return new WaitForSeconds(portalAnimationTime);

        // Dash forward
        float dashDuration = 1f;
        Vector2 targetPosition = entryPosition;
        float timeElapsed = 0f;

        while (timeElapsed < dashDuration && !hasTeleported)
        {
            transform.position = Vector2.Lerp(transform.position, targetPosition, timeElapsed / dashDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // If we never hit the portal (rare case), teleport anyway
        if (!hasTeleported)
        {
            transform.position = targetPosition;
            TeleportToAlternateWorld();
        }

        // Wait a bit before destroying the entry portal
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;

        // Destroy the entry portal
        Destroy(entryPortal);

        /*yield return new WaitForSeconds(dashingCooldown);
        canDash = true;*/
    }

    public void TeleportToAlternateWorld()
    {
        if (!hasTeleported)
        {
            hasTeleported = true;

            // Calculate the position where the exit portal should appear in the new world
            Vector2 exitPortalPosition;
            Vector2 playerExitPosition;


            // Stop title screen music and play cyber city music
            if (audioManager != null)
            {
                audioManager.Stop("TitleScreen");
                audioManager.Play("CyberCityMusic");
            }

            if (!isInAlternateLevel)
            {
                // Going to alternate world
                exitPortalPosition = (Vector2)transform.position + levelOffset;
                // Store original position before teleporting
                originalLevelPosition = transform.position;

                // Player should appear slightly to the right/left of the exit portal
                Vector2 exitDirection = isFacingRight ? Vector2.right : Vector2.left;
                playerExitPosition = exitPortalPosition + exitDirection * 1.5f;
            }
            else
            {
                // Returning to original world
                exitPortalPosition = originalLevelPosition;

                // Player should appear slightly to the right/left of the exit portal
                Vector2 exitDirection = isFacingRight ? Vector2.right : Vector2.left;
                playerExitPosition = exitPortalPosition + exitDirection * 1.5f;
            }

            // Create the exit portal at the destination position
            GameObject exitPortal = Instantiate(portalPrefab, exitPortalPosition, Quaternion.identity);
            Animator exitPortalAnimator = exitPortal.GetComponent<Animator>();
            exitPortalAnimator.Play("Portal");

            // Switch world state
            isInAlternateLevel = !isInAlternateLevel;

            // Start a coroutine to handle the player's emergence from the portal
            StartCoroutine(EmergeThroughPortal(playerExitPosition, exitPortal));

            // Switch world colors or background
            SwitchWorld();
        }
    }

    private IEnumerator EmergeThroughPortal(Vector2 playerExitPosition, GameObject exitPortal)
    {
        // First teleport player to exit portal position
        transform.position = exitPortal.transform.position;

        // Give the portal a moment to be visible
        yield return new WaitForSeconds(0.2f);

        // Move player out from the portal
        float emergeTime = 0.3f;
        float emergeTimer = 0;
        Vector2 startPos = transform.position;

        // Play player emerge animation if you have one
        // animator.Play("EmergingAnimation");

        while (emergeTimer < emergeTime)
        {
            transform.position = Vector2.Lerp(startPos, playerExitPosition, emergeTimer / emergeTime);
            emergeTimer += Time.deltaTime;
            yield return null;
        }

        // Ensure player ends at the exact destination
        transform.position = playerExitPosition;

        // Reset dashing state after portal emergence
        tr.emitting = false;
        rb.gravityScale = 10f; // just in case
        isDashing = false;
        animator.Play("PlayerIdle"); // or whatever your idle/walk default is

        // Destroy the exit portal after a short delay
        yield return new WaitForSeconds(0.5f);
        Destroy(exitPortal);
    }

    private void SwitchWorld()
    {
        if (isInAlternateLevel)
        {
            // Switch to alternate background
            if (normalBackground != null) normalBackground.SetActive(false);
            if (alternateBackground != null) alternateBackground.SetActive(true);
        }
        else
        {
            // Switch to normal background
            if (normalBackground != null) normalBackground.SetActive(true);
            if (alternateBackground != null) alternateBackground.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void PlayerFollowPlatform(bool isOnPlatform, float speed)
    {
        playerOnPlatform = isOnPlatform;
        platformSpeed = speed;
    }

    public void SetDashUnlocked(bool unlocked)
    {
        canDash = unlocked;
    }

    public void PlayDuckQuackAnimation()
    {
        animator.SetBool("IsQuacking", true);
    }

    public void StopDuckQuackAnimation()
    {
        animator.SetBool("IsQuacking", false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PathSwitchZone"))
        {
            inPathSwitchZone = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PathSwitchZone"))
        {
            inPathSwitchZone = false;
        }
    }

    void SwitchToHillPath()
    {
        onHill = true;
        gameObject.layer = LayerMask.NameToLayer("Background");
        spriteRenderer.sortingOrder = -1;
        transform.position += new Vector3(0, 0.1f, 0); // slight move for visual effect
    }

    void SwitchToGroundPath()
    {
        onHill = false;
        gameObject.layer = LayerMask.NameToLayer("Foreground");
        spriteRenderer.sortingOrder = 0;
        transform.position -= new Vector3(0, 0.1f, 0);
    }
}