using UnityEngine;

public class BuracoMorte : MonoBehaviour
{
    public float velocidadeQueda = -1.5f;
    public float gravidadeDuranteQueda = 0.2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, velocidadeQueda);
                rb.gravityScale = gravidadeDuranteQueda;
            }

            PlayerMovement movimento = other.GetComponent<PlayerMovement>();

            if (movimento != null)
            {
                movimento.enabled = false;
            }

            Vida vida = other.GetComponent<Vida>();

            if (vida != null)
            {
                vida.Morrer();
            }
        }
    }
}
