using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [SerializeField] private Animator anim;
    public Animator animator;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioClip somMovimentoCorpo;
    public AudioClip somAtaque;
    public AudioClip[] sonsPassosNeve;
    [Range(0f, 1f)] public float volumeMovimentoCorpo = 0.32f;
    [Range(0f, 1f)] public float volumeAtaque = 0.85f;
    [Range(0f, 1f)] public float volumePassosNeve = 0.52f;
    public float intervaloPassosNeve = 0.32f;
    public bool audioMovimento2D = true;

    private Rigidbody2D rb;

    private float horizontal;
    private bool noChao;
    private bool encostandoParedeEsquerda;
    private bool encostandoParedeDireita;

    private Vector3 originalScale;
    private bool facingRight = true;

    // ATAQUE
    public Transform pontoAtaque;
    public float raioAtaque = 1f;
    public int danoAtaque = 1;
    public LayerMask layerInimigo;

    private bool isAttacking;
    private int bloqueiosAtaque;
    private int indicePassoNeve;
    private float proximoPassoNeve;

    public bool AtaqueBloqueado => bloqueiosAtaque > 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        ConfigurarAudioSource();

        originalScale = transform.localScale;
    }

    void Update()
    {
        // Animações de pulo e queda
        animator.SetBool("Pulando", !noChao && rb.linearVelocity.y > 0.1f);
        animator.SetBool("Caindo", !noChao && rb.linearVelocity.y < -0.1f);

        if (isAttacking && !noChao)
        {
            isAttacking = false;
        }

        if (isAttacking)
        {
            PararSomMovimentoCorpo();
            return;
        }

        horizontal = Input.GetAxis("Horizontal") + MobileControlsInput.Horizontal;
        horizontal = Mathf.Clamp(horizontal, -1f, 1f);

        animator.SetFloat("horizontalAnim", Mathf.Abs(horizontal));

        Flip(horizontal);

        if (noChao && (Input.GetKeyDown(KeyCode.Space) || MobileControlsInput.ConsumirPulo()))
        {
            Jump();
        }

        Atacar();
        AtualizarSomMovimentoCorpo();
        AtualizarSomPassosNeve();
    }

    void FixedUpdate()
    {
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float movimentoHorizontal = horizontal;

        if (!noChao)
        {
            if (encostandoParedeDireita && movimentoHorizontal > 0f)
                movimentoHorizontal = 0f;
            else if (encostandoParedeEsquerda && movimentoHorizontal < 0f)
                movimentoHorizontal = 0f;
        }

        rb.linearVelocity = new Vector2(movimentoHorizontal * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        noChao = false;
        PararSomMovimentoCorpo();

        animator.SetBool("Pulando", true);
        animator.SetBool("Caindo", false);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("chao"))
        {
            foreach (ContactPoint2D contact in col.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    noChao = true;
                    animator.SetBool("Pulando", false);
                    animator.SetBool("Caindo", false);
                }

                AtualizarContatoParede(contact);
            }
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        encostandoParedeEsquerda = false;
        encostandoParedeDireita = false;

        if (col.gameObject.CompareTag("chao"))
        {
            bool tocandoChao = false;

            foreach (ContactPoint2D contact in col.contacts)
            {
                if (contact.normal.y > 0.5f)
                    tocandoChao = true;

                AtualizarContatoParede(contact);
            }

            if (tocandoChao)
            {
                noChao = true;
                animator.SetBool("Pulando", false);
                animator.SetBool("Caindo", false);
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("chao"))
        {
            noChao = false;
            encostandoParedeEsquerda = false;
            encostandoParedeDireita = false;
        }
    }

    private void AtualizarContatoParede(ContactPoint2D contact)
    {
        if (contact.normal.x > 0.45f)
            encostandoParedeEsquerda = true;
        else if (contact.normal.x < -0.45f)
            encostandoParedeDireita = true;
    }

    void Flip(float horizontalInput)
    {
        if (isAttacking) return;

        if (horizontalInput < 0 && facingRight)
        {
            facingRight = false;
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
        else if (horizontalInput > 0 && !facingRight)
        {
            facingRight = true;
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        }
    }

    private void Atacar()
    {
        if (AtaqueBloqueado)
        {
            MobileControlsInput.ConsumirAtaque();
            return;
        }

        if (noChao && !isAttacking && (Input.GetKeyDown(KeyCode.Z) || MobileControlsInput.ConsumirAtaque()))
        {
            isAttacking = true;
            PararSomMovimentoCorpo();
            TocarSomAtaque();

            rb.linearVelocity = Vector2.zero;

            if (anim != null)
            {
                anim.SetTrigger("Atacar");
            }

            Collider2D[] inimigos = layerInimigo.value == 0
                ? Physics2D.OverlapCircleAll(pontoAtaque.position, raioAtaque)
                : Physics2D.OverlapCircleAll(pontoAtaque.position, raioAtaque, layerInimigo);

            HashSet<InimigoVida> inimigosAcertados = new HashSet<InimigoVida>();

            foreach (Collider2D inimigo in inimigos)
            {
                InimigoVida vidaInimigo = inimigo.GetComponentInParent<InimigoVida>();

                if (vidaInimigo != null && inimigosAcertados.Add(vidaInimigo))
                    vidaInimigo.ReceberDano(danoAtaque);
            }

            StartCoroutine(ResetAtaque());
        }
    }

    IEnumerator ResetAtaque()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    public void FimAtaque()
    {
        isAttacking = false;
    }

    public void DefinirAtaqueBloqueado(bool bloqueado)
    {
        if (bloqueado)
            bloqueiosAtaque++;
        else
            bloqueiosAtaque = Mathf.Max(0, bloqueiosAtaque - 1);

        if (AtaqueBloqueado)
            isAttacking = false;
    }

    private void ConfigurarAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = audioMovimento2D ? 0f : 1f;
    }

    private void AtualizarSomMovimentoCorpo()
    {
        if (audioSource == null || somMovimentoCorpo == null)
            return;

        bool andandoNoChao = noChao && Mathf.Abs(horizontal) > 0.08f && !isAttacking;

        if (!andandoNoChao)
        {
            PararSomMovimentoCorpo();
            return;
        }

        if (audioSource.clip == somMovimentoCorpo && audioSource.isPlaying)
            return;

        audioSource.clip = somMovimentoCorpo;
        audioSource.loop = true;
        audioSource.volume = volumeMovimentoCorpo;
        audioSource.spatialBlend = audioMovimento2D ? 0f : 1f;
        audioSource.Play();
    }

    private void PararSomMovimentoCorpo()
    {
        if (audioSource == null || audioSource.clip != somMovimentoCorpo)
            return;

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }

    private void TocarSomAtaque()
    {
        if (audioSource == null || somAtaque == null)
            return;

        audioSource.PlayOneShot(somAtaque, volumeAtaque);
    }

    private void AtualizarSomPassosNeve()
    {
        if (audioSource == null || sonsPassosNeve == null || sonsPassosNeve.Length == 0)
            return;

        bool andandoNoChao = noChao && Mathf.Abs(horizontal) > 0.08f && !isAttacking;

        if (!andandoNoChao || Time.time < proximoPassoNeve)
            return;

        AudioClip passo = sonsPassosNeve[indicePassoNeve % sonsPassosNeve.Length];
        indicePassoNeve++;

        if (passo != null)
            audioSource.PlayOneShot(passo, volumePassosNeve);

        float velocidadeRelativa = Mathf.Clamp(Mathf.Abs(horizontal), 0.65f, 1.25f);
        proximoPassoNeve = Time.time + intervaloPassosNeve / velocidadeRelativa;
    }

    private void OnDisable()
    {
        PararSomMovimentoCorpo();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (somMovimentoCorpo == null)
            somMovimentoCorpo = CarregarClipEditor("HeroSmallBodyMovement1");

        if (somAtaque == null)
            somAtaque = CarregarClipEditor("HeroPunch");

        if (sonsPassosNeve == null || sonsPassosNeve.Length == 0)
        {
            AudioClip passo1 = CarregarClipEditor("HeroFootStepSnow1");
            AudioClip passo2 = CarregarClipEditor("HeroFootStepSnow2");

            if (passo1 != null && passo2 != null)
                sonsPassosNeve = new[] { passo1, passo2 };
        }

        if (audioSource != null)
            audioSource.spatialBlend = audioMovimento2D ? 0f : 1f;
    }

    private AudioClip CarregarClipEditor(string nome)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(nome + " t:AudioClip");

        foreach (string guid in guids)
        {
            string caminho = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            if (caminho.Contains("/Hero/") && System.IO.Path.GetFileNameWithoutExtension(caminho) == nome)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(caminho);
        }

        return null;
    }
#endif

    void OnDrawGizmosSelected()
    {
        if (pontoAtaque == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pontoAtaque.position, raioAtaque);
    }
}
