using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    public float runSpeed = 40f;
    float horizontalMove = 0f;
    [SerializeField] private float JumpTimeCounter;
    public float JumpTime;
    public bool isJumping;
    public float jumpBoost;

    public float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;
	private Animator playerAnimator;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		playerAnimator = GetComponent<Animator>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();
	}

    private void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        bool wasGrounded = m_Grounded;
        m_Grounded = false;
        playerAnimator.SetBool("isGrounded", m_Grounded);

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                playerAnimator.SetBool("isGrounded", m_Grounded);
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }

        if (m_Grounded)
        {
            if (Input.GetButton("Jump") || Input.GetKey(KeyCode.W))
            {
                m_Grounded = false;
                m_Rigidbody2D.AddForce(Vector2.up * m_JumpForce);
                isJumping = true;
            }
            else
            {
                isJumping = false;
                JumpTimeCounter = JumpTime;
            }
        }

        if (horizontalMove != 0 && m_Grounded)
        {
            playerAnimator.SetBool("isWalking", true);
        }
        else if (!m_Grounded || horizontalMove == 0 || isJumping)
        {
            playerAnimator.SetBool("isWalking", false);
        }
    }

    private void FixedUpdate()
	{
        Move(horizontalMove * Time.fixedDeltaTime);
	}


    public void Move(float move)
    {

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }

        // If the player should continue to jump...
        if (isJumping)
        {
            if (Input.GetButton("Jump") || Input.GetKey(KeyCode.W))
            {
                if (JumpTimeCounter > 0)
                {
                    m_Rigidbody2D.velocity += Vector2.up * jumpBoost;
                    JumpTimeCounter -= Time.deltaTime;
                }
                else
                {
                    isJumping = false;
                }
            }
        }
	}


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
