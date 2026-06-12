using UnityEngine;

public class AtivarSomCorvo : MonoBehaviour
{
    public AudioSource corvoAudio;
    public CorvoVoo corvo;

    private bool ativou = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player") && !ativou)
        {
            ativou = true;

            // 🔊 som
            if (corvoAudio != null)
                corvoAudio.Play();

            // 🐦 voar
            if (corvo != null)
                corvo.Voar();
        }
    }
}