using UnityEngine;

public class Espinhos : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            Vida vida = other.GetComponent<Vida>();

            if (vida != null)
            {
                for (int i = 0; i < 10; i++) // tira tudo
                {
                    vida.PerderVida();
                }
            }
        }
    }
}