using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InimigoVida))]
public class InimigoContatoDano : MonoBehaviour
{
    [Header("Alvo")]
    public Transform player;
    public float distanciaDetectar = 6f;
    public bool focarPlayerAoChegarNasCostas = true;
    public float distanciaSentirPlayerNasCostas = 3.2f;
    public float alturaSentirPlayerNasCostas = 1.6f;
    public float distanciaParaPerderFoco = 9f;

    [Header("Movimento")]
    public float velocidadePerseguindo = 2.4f;
    public float velocidadePatrulha = 1.1f;
    public Transform pontoPatrulhaEsquerda;
    public Transform pontoPatrulhaDireita;
    public float distanciaPatrulhaAutomatica = 4f;
    public float distanciaParaVirarPatrulha = 0.15f;
    public float tempoPresoParaVirar = 0.25f;

    [Header("Dano por contato")]
    public int danoContato = 1;
    public float intervaloDano = 0.9f;
    public float intervaloDanoGrudado = 5f;
    public bool ignorarColisaoComPlayer = false;

    [Header("Grudar no player")]
    public bool grudarNoPlayerAoEncostar = true;
    public Vector2 offsetGrudado = new Vector2(0.2f, 0.1f);
    public int cliquesParaDesgrudar = 5;
    public float janelaCliquesDesgrudar = 1.25f;
    public float distanciaAoDesgrudar = 2.4f;
    public float forcaEmpurraoAoDesgrudar = 7f;
    public float tempoRecuoDepoisDeSoltar = 0.7f;
    public float tempoSemGrudarDepoisDeSoltar = 2f;
    public float velocidadePlayerGrudado = 4f;

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioClip somAndando;
    [Range(0f, 1f)] public float volumeAndando = 0.38f;
    public bool audio3D = true;
    public float distanciaMinimaAudio = 1.5f;
    public float distanciaMaximaAudio = 12f;

    [Header("Visual")]
    public Transform visual;
    public bool inverterDirecaoVisual = true;

    [Header("Animator")]
    public string boolRun = "Run";
    public string estadoIdle = "Idle";
    public string estadoRun = "Run";
    public bool tocarEstadosDireto = true;

    private Rigidbody2D rb;
    private Animator anim;
    private InimigoVida vida;
    private Collider2D[] collidersInimigo;
    private Vector3 escalaOriginal;
    private int direcaoPatrulha = 1;
    private float limitePatrulhaEsquerda;
    private float limitePatrulhaDireita;
    private float proximoDano;
    private float proximoGrude;
    private float ultimaPosicaoX;
    private float tempoPreso;
    private string estadoAtual;
    private bool grudadoNoPlayer;
    private int cliquesDesgrudar;
    private float ultimoCliqueDesgrudar;
    private float recuandoAte;
    private float gravidadeOriginal;
    private RigidbodyType2D bodyTypeOriginal;
    private PlayerMovement movimentoPlayer;
    private float velocidadeOriginalPlayer;
    private bool playerLentoAplicado;
    private bool ataquePlayerBloqueado;
    private bool focandoPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        vida = GetComponent<InimigoVida>();
        collidersInimigo = GetComponentsInChildren<Collider2D>();
        anim = GetComponent<Animator>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (visual == null && anim != null)
            visual = anim.transform;

        if (visual == null)
            visual = transform;

        escalaOriginal = visual.localScale;
        gravidadeOriginal = rb.gravityScale;
        bodyTypeOriginal = rb.bodyType;
        rb.freezeRotation = true;
        ConfigurarAudioSource();
    }

    private void Start()
    {
        EncontrarPlayer();
        ConfigurarColisaoComPlayer();
        ConfigurarLimitesPatrulha();
        ultimaPosicaoX = transform.position.x;
    }

    private void Update()
    {
        if (vida != null && vida.EstaMorto)
        {
            PararSomAndando();
            MobileControlsInput.DefinirEscaparGrudeDisponivel(false);
            return;
        }

        if (player == null)
        {
            EncontrarPlayer();
            ConfigurarColisaoComPlayer();
        }

        if (grudadoNoPlayer)
        {
            AtualizarGrude();
            return;
        }

        if (Time.time < recuandoAte)
        {
            TocarRun();
            return;
        }

        if (Time.time < proximoGrude)
        {
            Patrulhar();
            return;
        }

        if (PlayerEntrouNoFoco())
            focandoPlayer = true;

        if (focandoPlayer && player != null)
        {
            if (Vector2.Distance(transform.position, player.position) > distanciaParaPerderFoco)
                focandoPlayer = false;
            else
            {
                PerseguirPlayer();
                return;
            }
        }

        if (player != null && Vector2.Distance(transform.position, player.position) <= distanciaDetectar)
        {
            focandoPlayer = true;
            PerseguirPlayer();
            return;
        }

        Patrulhar();
    }

    private void PerseguirPlayer()
    {
        if (player == null)
            return;

        float direcao = Mathf.Sign(player.position.x - transform.position.x);
        Mover(direcao, velocidadePerseguindo);
    }

    private bool PlayerEntrouNoFoco()
    {
        if (player == null)
            return false;

        if (Vector2.Distance(transform.position, player.position) <= distanciaDetectar)
            return true;

        if (!focarPlayerAoChegarNasCostas)
            return false;

        Vector2 diferenca = player.position - transform.position;

        if (Mathf.Abs(diferenca.x) > distanciaSentirPlayerNasCostas || Mathf.Abs(diferenca.y) > alturaSentirPlayerNasCostas)
            return false;

        return Mathf.Sign(diferenca.x) != Mathf.Sign(direcaoPatrulha);
    }

    private void Patrulhar()
    {
        if (direcaoPatrulha > 0 && transform.position.x >= limitePatrulhaDireita - distanciaParaVirarPatrulha)
            direcaoPatrulha = -1;
        else if (direcaoPatrulha < 0 && transform.position.x <= limitePatrulhaEsquerda + distanciaParaVirarPatrulha)
            direcaoPatrulha = 1;

        VirarSeFicouPreso();
        Mover(direcaoPatrulha, velocidadePatrulha);
    }

    private void Mover(float direcao, float velocidade)
    {
        if (Mathf.Abs(direcao) < 0.01f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            TocarIdle();
            return;
        }

        rb.linearVelocity = new Vector2(Mathf.Sign(direcao) * velocidade, rb.linearVelocity.y);
        Virar(Mathf.Sign(direcao));
        TocarRun();
        TocarSomAndando();
    }

    private void CausarDanoSePlayer(Collider2D other)
    {
        if (Time.time < proximoDano || other == null || vida != null && vida.EstaMorto)
            return;

        Vida vidaPlayer = other.GetComponentInParent<Vida>();

        if (vidaPlayer == null)
            return;

        proximoDano = Time.time + (grudadoNoPlayer ? intervaloDanoGrudado : intervaloDano);
        vidaPlayer.PerderVida();
    }

    private void TentarGrudar(Collider2D other)
    {
        if (!grudarNoPlayerAoEncostar || grudadoNoPlayer || Time.time < proximoGrude || other == null)
            return;

        Vida vidaPlayer = other.GetComponentInParent<Vida>();

        if (vidaPlayer == null)
            return;

        if (player == null)
            player = vidaPlayer.transform;

        GrudarNoPlayer(vidaPlayer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TentarGrudar(collision.collider);
        CausarDanoSePlayer(collision.collider);
        VirarSeBateuEmParede(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TentarGrudar(collision.collider);
        CausarDanoSePlayer(collision.collider);
        VirarSeBateuEmParede(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TentarGrudar(other);
        CausarDanoSePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TentarGrudar(other);
        CausarDanoSePlayer(other);
    }

    private void GrudarNoPlayer(Vida vidaPlayer)
    {
        if (player == null)
            return;

        grudadoNoPlayer = true;
        cliquesDesgrudar = 0;
        ultimoCliqueDesgrudar = 0f;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        ConfigurarIgnorarColisaoPlayer(true);
        AplicarLentidaoPlayer();
        BloquearAtaquePlayer();
        
        if (vidaPlayer != null)
            vidaPlayer.PerderVida();

        proximoDano = Time.time + Mathf.Max(0.1f, intervaloDanoGrudado);
        MobileControlsInput.DefinirEscaparGrudeDisponivel(true);
        TocarIdle();
    }

    private void AtualizarGrude()
    {
        if (player == null)
        {
            Desgrudar();
            return;
        }

        Vector2 lado = transform.position.x >= player.position.x ? Vector2.right : Vector2.left;
        Vector2 offset = new Vector2(Mathf.Abs(offsetGrudado.x) * lado.x, offsetGrudado.y);
        transform.position = (Vector2)player.position + offset;
        rb.linearVelocity = Vector2.zero;

        CausarDanoSePlayer(player.GetComponentInChildren<Collider2D>());

        if (MobileControlsInput.ConsumirEscaparGrude() || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            RegistrarCliqueDesgrudar();
    }

    private void RegistrarCliqueDesgrudar()
    {
        if (Time.time - ultimoCliqueDesgrudar > janelaCliquesDesgrudar)
            cliquesDesgrudar = 0;

        ultimoCliqueDesgrudar = Time.time;
        cliquesDesgrudar++;

        if (cliquesDesgrudar >= Mathf.Max(1, cliquesParaDesgrudar))
            Desgrudar();
    }

    private void Desgrudar()
    {
        grudadoNoPlayer = false;
        cliquesDesgrudar = 0;
        proximoGrude = Time.time + tempoSemGrudarDepoisDeSoltar;
        recuandoAte = Time.time + tempoRecuoDepoisDeSoltar;
        MobileControlsInput.DefinirEscaparGrudeDisponivel(false);
        LiberarAtaquePlayer();
        RestaurarVelocidadePlayer();

        rb.bodyType = bodyTypeOriginal;
        rb.gravityScale = gravidadeOriginal;

        if (player != null)
        {
            float lado = transform.position.x >= player.position.x ? 1f : -1f;
            transform.position += Vector3.right * lado * distanciaAoDesgrudar;
            rb.linearVelocity = new Vector2(lado * forcaEmpurraoAoDesgrudar, rb.linearVelocity.y);
            direcaoPatrulha = lado > 0f ? 1 : -1;
        }

        ConfigurarIgnorarColisaoPlayer(false);
    }

    private void AplicarLentidaoPlayer()
    {
        if (playerLentoAplicado || player == null)
            return;

        movimentoPlayer = player.GetComponent<PlayerMovement>();

        if (movimentoPlayer == null)
            movimentoPlayer = player.GetComponentInParent<PlayerMovement>();

        if (movimentoPlayer == null)
            return;

        velocidadeOriginalPlayer = movimentoPlayer.moveSpeed;
        movimentoPlayer.moveSpeed = velocidadePlayerGrudado;
        playerLentoAplicado = true;
    }

    private void RestaurarVelocidadePlayer()
    {
        if (!playerLentoAplicado || movimentoPlayer == null)
            return;

        movimentoPlayer.moveSpeed = velocidadeOriginalPlayer;
        playerLentoAplicado = false;
        movimentoPlayer = null;
    }

    private void BloquearAtaquePlayer()
    {
        if (ataquePlayerBloqueado)
            return;

        if (movimentoPlayer == null && player != null)
            movimentoPlayer = player.GetComponentInParent<PlayerMovement>();

        if (movimentoPlayer == null)
            return;

        movimentoPlayer.DefinirAtaqueBloqueado(true);
        ataquePlayerBloqueado = true;
    }

    private void LiberarAtaquePlayer()
    {
        if (!ataquePlayerBloqueado || movimentoPlayer == null)
            return;

        movimentoPlayer.DefinirAtaqueBloqueado(false);
        ataquePlayerBloqueado = false;
    }

    private void EncontrarPlayer()
    {
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
            player = playerMovement.transform;
    }

    private void ConfigurarLimitesPatrulha()
    {
        if (pontoPatrulhaEsquerda != null && pontoPatrulhaDireita != null)
        {
            limitePatrulhaEsquerda = Mathf.Min(pontoPatrulhaEsquerda.position.x, pontoPatrulhaDireita.position.x);
            limitePatrulhaDireita = Mathf.Max(pontoPatrulhaEsquerda.position.x, pontoPatrulhaDireita.position.x);
            return;
        }

        float metade = Mathf.Max(0.5f, distanciaPatrulhaAutomatica) * 0.5f;
        limitePatrulhaEsquerda = transform.position.x - metade;
        limitePatrulhaDireita = transform.position.x + metade;
    }

    private void ConfigurarColisaoComPlayer()
    {
        if (!ignorarColisaoComPlayer || player == null || collidersInimigo == null)
            return;

        ConfigurarIgnorarColisaoPlayer(true);
    }

    private void ConfigurarIgnorarColisaoPlayer(bool ignorar)
    {
        if (player == null || collidersInimigo == null)
            return;

        Collider2D[] collidersPlayer = player.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D colliderInimigo in collidersInimigo)
        {
            if (colliderInimigo == null || colliderInimigo.isTrigger)
                continue;

            foreach (Collider2D colliderPlayer in collidersPlayer)
            {
                if (colliderPlayer != null && !colliderPlayer.isTrigger)
                    Physics2D.IgnoreCollision(colliderInimigo, colliderPlayer, ignorar);
            }
        }
    }

    private void OnDisable()
    {
        if (grudadoNoPlayer)
            Desgrudar();

        LiberarAtaquePlayer();
        PararSomAndando();
        MobileControlsInput.DefinirEscaparGrudeDisponivel(false);
    }

    private void VirarSeFicouPreso()
    {
        float movimentoEsperado = Mathf.Sign(direcaoPatrulha);
        float movimentoReal = transform.position.x - ultimaPosicaoX;

        if (Mathf.Abs(movimentoReal) < 0.004f || Mathf.Sign(movimentoReal) != movimentoEsperado)
            tempoPreso += Time.deltaTime;
        else
            tempoPreso = 0f;

        ultimaPosicaoX = transform.position.x;

        if (tempoPreso < tempoPresoParaVirar)
            return;

        direcaoPatrulha *= -1;
        tempoPreso = 0f;
    }

    private void VirarSeBateuEmParede(Collision2D collision)
    {
        if (vida != null && vida.EstaMorto)
            return;

        foreach (ContactPoint2D contato in collision.contacts)
        {
            if (direcaoPatrulha > 0 && contato.normal.x < -0.45f)
            {
                direcaoPatrulha = -1;
                tempoPreso = 0f;
                return;
            }

            if (direcaoPatrulha < 0 && contato.normal.x > 0.45f)
            {
                direcaoPatrulha = 1;
                tempoPreso = 0f;
                return;
            }
        }
    }

    private void Virar(float direcao)
    {
        if (Mathf.Abs(direcao) < 0.01f)
            return;

        float sinal = Mathf.Sign(direcao);

        if (inverterDirecaoVisual)
            sinal *= -1f;

        float x = Mathf.Abs(escalaOriginal.x) * sinal;
        visual.localScale = new Vector3(x, escalaOriginal.y, escalaOriginal.z);
    }

    private void TocarIdle()
    {
        PararSomAndando();

        if (SetBoolSeExistir(boolRun, false))
            return;

        TocarEstadoDireto(estadoIdle, false);
    }

    private void TocarRun()
    {
        if (SetBoolSeExistir(boolRun, true))
            return;

        TocarEstadoDireto(estadoRun, false);
    }

    private void ConfigurarAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = audio3D ? 1f : 0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = Mathf.Max(0.1f, distanciaMinimaAudio);
        audioSource.maxDistance = Mathf.Max(audioSource.minDistance + 0.1f, distanciaMaximaAudio);
    }

    private void TocarSomAndando()
    {
        if (vida != null && vida.EstaMorto || audioSource == null || somAndando == null || grudadoNoPlayer)
            return;

        if (audioSource.clip == somAndando && audioSource.isPlaying)
            return;

        audioSource.clip = somAndando;
        audioSource.loop = true;
        audioSource.volume = volumeAndando;
        audioSource.Play();
    }

    private void PararSomAndando()
    {
        if (audioSource == null || audioSource.clip != somAndando)
            return;

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (somAndando == null)
            somAndando = CarregarClipEditor("IceGolemWalk");

        if (audioSource != null)
        {
            audioSource.spatialBlend = audio3D ? 1f : 0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = Mathf.Max(0.1f, distanciaMinimaAudio);
            audioSource.maxDistance = Mathf.Max(audioSource.minDistance + 0.1f, distanciaMaximaAudio);
        }
    }

    private AudioClip CarregarClipEditor(string nome)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(nome + " t:AudioClip");

        foreach (string guid in guids)
        {
            string caminho = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            if (caminho.Contains("/IceGolem/") && System.IO.Path.GetFileNameWithoutExtension(caminho) == nome)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(caminho);
        }

        return null;
    }
#endif

    private bool SetBoolSeExistir(string nome, bool valor)
    {
        if (anim == null || string.IsNullOrEmpty(nome))
            return false;

        foreach (AnimatorControllerParameter parametro in anim.parameters)
        {
            if (parametro.name == nome && parametro.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(nome, valor);
                return true;
            }
        }

        return false;
    }

    private void TocarEstadoDireto(string nomeEstado, bool reiniciarMesmoEstado)
    {
        if (!tocarEstadosDireto || anim == null || string.IsNullOrEmpty(nomeEstado))
            return;

        if (!reiniciarMesmoEstado && estadoAtual == nomeEstado)
            return;

        anim.Play(nomeEstado, 0, 0f);
        estadoAtual = nomeEstado;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaDetectar);

        Gizmos.color = Color.yellow;
        float esquerda = pontoPatrulhaEsquerda != null ? pontoPatrulhaEsquerda.position.x : limitePatrulhaEsquerda;
        float direita = pontoPatrulhaDireita != null ? pontoPatrulhaDireita.position.x : limitePatrulhaDireita;
        Vector3 a = new Vector3(esquerda, transform.position.y, transform.position.z);
        Vector3 b = new Vector3(direita, transform.position.y, transform.position.z);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.15f);
        Gizmos.DrawWireSphere(b, 0.15f);
    }
}
