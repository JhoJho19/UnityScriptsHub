using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private Collider2D colliderForDestroy;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] bool movingRight = true;

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 force = Vector2.zero;

        if (movingRight)
        {
            force = Vector2.right;
        }
        else
        {
            force = Vector2.left;
        }

        rb.AddForce(force * speed * Time.fixedDeltaTime, ForceMode2D.Impulse);
    }

    public void Flip()
    {
        movingRight = !movingRight;
        rb.velocity = Vector2.zero;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyFlipper"))
        {
            Flip();
        }
    }
}
