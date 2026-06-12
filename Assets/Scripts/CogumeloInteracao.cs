using UnityEngine;
using TMPro;

public class CogumeloInteracao : MonoBehaviour
{
    public GameObject textoInteracao; // "Pressione E"
    

public TextMeshProUGUI textoFala;

    public string[] falas;
    private int indice = 0;

    private bool playerPerto = false;
    private bool dialogoAtivo = false;

    void Update()
    {
        if (playerPerto && (Input.GetKeyDown(KeyCode.E) || MobileControlsInput.ConsumirInteracao()))
        {
            Interagir();
        }
    }

    void Interagir()
    {
        if (!dialogoAtivo)
            IniciarDialogo();
        else
            ProximaFala();
    }

    void IniciarDialogo()
    {
        dialogoAtivo = true;
        indice = 0;

        textoFala.gameObject.SetActive(true);
        textoFala.text = falas[indice];
    }

    void ProximaFala()
    {
        indice++;

        if (indice < falas.Length)
        {
            textoFala.text = falas[indice];
        }
        else
        {
            EncerrarDialogo();
        }
    }

    void EncerrarDialogo()
    {
        dialogoAtivo = false;
        textoFala.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            playerPerto = true;
            if (textoInteracao != null)
                textoInteracao.SetActive(true);

            MobileControlsInput.DefinirInteracaoDisponivel(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            playerPerto = false;
            if (textoInteracao != null)
                textoInteracao.SetActive(false);

            MobileControlsInput.DefinirInteracaoDisponivel(false);
            EncerrarDialogo();
        }
    }
}
