using UnityEngine;
using UnityEngine.UI;

public class Vida : MonoBehaviour
{
    public Image[] coracoes;
    public string tagEspinhos = "espinhos";
    public bool morrerAoTocarEspinhos = true;

    public AudioClip somDano;
    public AudioClip somMorte;
    public AudioSource audioSource;
    public TelaMorteUI telaMorteUI;
    public VidaMascaraHud vidaMascaraHud;
    public int limiteVidasBonus = 3;
    public bool vidaExtraCuraSeNaoEstiverCheio = true;

    private float vidaAtual;
    private int vidasBonus;
    private Animator anim;

    private bool isDead = false;

    void Start()
    {
        vidaAtual = coracoes.Length * 2;
        anim = GetComponent<Animator>();

        if (telaMorteUI == null)
            telaMorteUI = FindFirstObjectByType<TelaMorteUI>(FindObjectsInactive.Include);

        if (vidaMascaraHud == null)
            vidaMascaraHud = FindFirstObjectByType<VidaMascaraHud>(FindObjectsInactive.Include);

        if (vidaMascaraHud != null)
        {
            vidaMascaraHud.Configurar(Mathf.RoundToInt(vidaAtual));
            vidaMascaraHud.EsconderCoracoes(coracoes);
        }

        AtualizarUI();
    }

    public void PerderVida()
    {
        if (isDead) return;

        if (vidaAtual <= 0) return;

        bool danoConsumiuBonus = vidaAtual > VidaMaximaBase();

        vidaAtual -= 1;

        if (danoConsumiuBonus && vidasBonus > 0)
            vidasBonus--;

        if (vidaAtual < 0)
            vidaAtual = 0;

        AtualizarUI();

        // 🔊 som de dano
        if (audioSource != null && somDano != null)
            audioSource.PlayOneShot(somDano);

        // 💀 morreu
        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    public void GanharVidaExtra(int quantidade = 1)
    {
        if (isDead)
            return;

        int quantidadeFinal = Mathf.Max(1, quantidade);
        float vidaMaxima = VidaMaximaBase();
        int restante = quantidadeFinal;

        if (vidaExtraCuraSeNaoEstiverCheio && vidaAtual < vidaMaxima)
        {
            int cura = Mathf.Min(restante, Mathf.RoundToInt(vidaMaxima - vidaAtual));
            vidaAtual += cura;
            restante -= cura;
        }

        if (restante > 0)
        {
            int bonusAdicionar = Mathf.Min(restante, Mathf.Max(0, limiteVidasBonus - vidasBonus));
            vidasBonus += bonusAdicionar;
            vidaAtual += bonusAdicionar;
        }

        AtualizarUI();
    }

    public void Morrer()
    {
        if (isDead) return;

        vidaAtual = 0;
        vidasBonus = 0;
        AtualizarUI();

        isDead = true;

        // 🎬 animação de morte
        if (anim != null)
        {
            anim.SetTrigger("AnimMorte");
        }

        // 🔊 som de morte
        if (audioSource != null && somMorte != null)
            audioSource.PlayOneShot(somMorte);

        if (telaMorteUI == null)
            telaMorteUI = FindFirstObjectByType<TelaMorteUI>(FindObjectsInactive.Include);

        if (telaMorteUI != null)
            telaMorteUI.MostrarTelaMorte(transform);
        else
            Debug.LogWarning("Player morreu, mas nenhum TelaMorteUI foi encontrado na cena.");
    }

    void AtualizarUI()
    {
        for (int i = 0; i < coracoes.Length; i++)
        {
            if (vidaAtual >= (i + 1) * 2)
                coracoes[i].fillAmount = 1f;
            else if (vidaAtual == (i * 2) + 1)
                coracoes[i].fillAmount = 0.5f;
            else
                coracoes[i].fillAmount = 0f;
        }

        if (vidaMascaraHud != null)
            vidaMascaraHud.Atualizar(vidaAtual, Mathf.RoundToInt(VidaMaximaBase()) + vidasBonus);
    }

    private float VidaMaximaBase()
    {
        return coracoes.Length * 2;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        VerificarEspinho(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        VerificarEspinho(collision.collider);
    }

    private void VerificarEspinho(Collider2D other)
    {
        if (!morrerAoTocarEspinhos || other == null || isDead)
            return;

        if (other.CompareTag(tagEspinhos))
            Morrer();
    }
}
