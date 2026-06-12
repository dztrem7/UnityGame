using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InimigoVida))]
public class LoboInimigo : MonoBehaviour
{
    [Header("Alvo")]
    public Transform player;
    public float distanciaDetectar = 7f;
    public float distanciaAtaque = 1.25f;

    [Header("Movimento")]
    public float velocidadePerseguindo = 3.2f;
    public float velocidadePatrulha = 1.4f;
    public Transform pontoPatrulhaEsquerda;
    public Transform pontoPatrulhaDireita;
    public float distanciaPatrulhaAutomatica = 5f;
    public float distanciaParaVirarPatrulha = 0.15f;
    public float tempoPresoParaVirar = 0.25f;

    [Header("Visual")]
    public Transform visual;
    public bool inverterDirecaoVisual = true;

    [Header("Ataque")]
    public Transform pontoAtaque;
    public float raioAtaque = 0.75f;
    public int danoAtaque = 1;
    public float intervaloAtaque = 1.1f;
    public float atrasoDanoAtaque = 0.28f;
    public float margemDanoPorProximidade = 0.45f;
    public LayerMask layerPlayer;
    public bool ignorarColisaoComPlayer = true;

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioSource audioSourceEfeitos;
    public AudioClip somMordida;
    public AudioClip somAndando;
    [Range(0f, 1f)] public float volumeMordida = 0.9f;
    [Range(0f, 1f)] public float volumeAndando = 0.38f;
    public bool audio3D = true;
    public float distanciaMinimaAudio = 1.5f;
    public float distanciaMaximaAudio = 12f;

    [Header("Animator")]
    public string boolRun = "Run";
    public string triggerAttack = "Attack";
    public string estadoIdle = "Idle";
    public string estadoRun = "Run";
    public string estadoAttack = "Attack";
    public bool tocarEstadosDireto = true;

    private Rigidbody2D rb;
    private Animator anim;
    private InimigoVida vida;
    private Collider2D[] collidersLobo;
    private Vector3 escalaOriginal;
    private int direcaoPatrulha = 1;
    private float proximoAtaque;
    private bool atacando;
    private string estadoAtual;
    private float limitePatrulhaEsquerda;
    private float limitePatrulhaDireita;
    private float ultimaPosicaoX;
    private float tempoPreso;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collidersLobo = GetComponentsInChildren<Collider2D>();
        anim = GetComponent<Animator>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        vida = GetComponent<InimigoVida>();
        PreencherSonsNoEditor();
        ConfigurarAudioSource();

        if (visual == null && anim != null)
            visual = anim.transform;

        if (visual == null)
            visual = transform;

        escalaOriginal = visual.localScale;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        EncontrarPlayer();
        ConfigurarColisaoComPlayer();

        if (pontoAtaque == null)
            pontoAtaque = transform;

        ConfigurarLimitesPatrulha();
        ultimaPosicaoX = transform.position.x;
    }

    private void Update()
    {
        if (vida != null && vida.EstaMorto)
            return;

        if (player == null)
        {
            EncontrarPlayer();
            ConfigurarColisaoComPlayer();
        }

        if (atacando)
            return;

        if (player != null)
        {
            float distancia = Vector2.Distance(transform.position, player.position);

            if (distancia <= distanciaAtaque)
            {
                TentarAtacar();
                return;
            }

            if (distancia <= distanciaDetectar)
            {
                PerseguirPlayer();
                return;
            }
        }

        Patrulhar();
    }

    private void PerseguirPlayer()
    {
        float direcao = Mathf.Sign(player.position.x - transform.position.x);
        Mover(direcao, velocidadePerseguindo);
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
            TocarIdle();
            PararSomAndando();
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(Mathf.Sign(direcao) * velocidade, rb.linearVelocity.y);
        Virar(Mathf.Sign(direcao));
        TocarRun();
        TocarSomAndando();
    }

    private void TentarAtacar()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        TocarIdle();
        PararSomAndando();

        if (player != null)
            Virar(Mathf.Sign(player.position.x - transform.position.x));

        if (Time.time < proximoAtaque)
            return;

        StartCoroutine(Atacar());
    }

    private IEnumerator Atacar()
    {
        atacando = true;
        proximoAtaque = Time.time + intervaloAtaque;
        TocarSomMordida();
        if (!SetTriggerSeExistir(triggerAttack))
            TocarEstadoDireto(estadoAttack, true);

        yield return new WaitForSeconds(atrasoDanoAtaque);
        CausarDanoNoPlayer();

        yield return new WaitForSeconds(Mathf.Max(0.05f, intervaloAtaque - atrasoDanoAtaque));
        atacando = false;
    }

    private void CausarDanoNoPlayer()
    {
        Collider2D[] acertos = layerPlayer.value == 0
            ? Physics2D.OverlapCircleAll(pontoAtaque.position, raioAtaque)
            : Physics2D.OverlapCircleAll(pontoAtaque.position, raioAtaque, layerPlayer);

        foreach (Collider2D acerto in acertos)
        {
            Vida vidaPlayer = acerto.GetComponentInParent<Vida>();

            if (vidaPlayer == null)
                continue;

            vidaPlayer.PerderVida();
            return;
        }

        if (player != null && Vector2.Distance(transform.position, player.position) <= distanciaAtaque + margemDanoPorProximidade)
        {
            Vida vidaPlayer = player.GetComponentInParent<Vida>();

            if (vidaPlayer != null)
                vidaPlayer.PerderVida();
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
        if (!ignorarColisaoComPlayer || player == null || collidersLobo == null)
            return;

        Collider2D[] collidersPlayer = player.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D colliderLobo in collidersLobo)
        {
            if (colliderLobo == null || colliderLobo.isTrigger)
                continue;

            foreach (Collider2D colliderPlayer in collidersPlayer)
            {
                if (colliderPlayer != null && !colliderPlayer.isTrigger)
                    Physics2D.IgnoreCollision(colliderLobo, colliderPlayer, true);
            }
        }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        VirarSeBateuEmParede(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        VirarSeBateuEmParede(collision);
    }

    private void VirarSeBateuEmParede(Collision2D collision)
    {
        if (atacando || vida != null && vida.EstaMorto)
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
        AplicarConfiguracaoAudio(audioSource);

        if (audioSourceEfeitos == null)
            audioSourceEfeitos = gameObject.AddComponent<AudioSource>();

        audioSourceEfeitos.playOnAwake = false;
        audioSourceEfeitos.loop = false;
        AplicarConfiguracaoAudio(audioSourceEfeitos);
    }

    private void TocarSomMordida()
    {
        if ((vida != null && vida.EstaMorto) || audioSourceEfeitos == null || somMordida == null)
            return;

        audioSourceEfeitos.PlayOneShot(somMordida, volumeMordida);
    }

    private void TocarSomAndando()
    {
        if ((vida != null && vida.EstaMorto) || audioSource == null || somAndando == null || atacando)
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

    private void AplicarConfiguracaoAudio(AudioSource source)
    {
        if (source == null)
            return;

        source.spatialBlend = audio3D ? 1f : 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = Mathf.Max(0.1f, distanciaMinimaAudio);
        source.maxDistance = Mathf.Max(source.minDistance + 0.1f, distanciaMaximaAudio);
    }

    private void OnDisable()
    {
        PararSomAndando();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        PreencherSonsNoEditor();

        if (audioSource != null)
        {
            AplicarConfiguracaoAudio(audioSource);
        }

        if (audioSourceEfeitos != null)
            AplicarConfiguracaoAudio(audioSourceEfeitos);
    }

    private void PreencherSonsNoEditor()
    {
        if (somMordida == null)
            somMordida = CarregarClipEditor("WolfBite");

        if (somAndando == null)
            somAndando = CarregarClipEditor("WolfWalk");
    }

    private AudioClip CarregarClipEditor(string nome)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(nome + " t:AudioClip");

        foreach (string guid in guids)
        {
            string caminho = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            if (caminho.Contains("/Wolf/") && System.IO.Path.GetFileNameWithoutExtension(caminho) == nome)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(caminho);
        }

        return null;
    }
#else
    private void PreencherSonsNoEditor()
    {
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

    private bool SetTriggerSeExistir(string nome)
    {
        if (anim == null || string.IsNullOrEmpty(nome))
            return false;

        foreach (AnimatorControllerParameter parametro in anim.parameters)
        {
            if (parametro.name == nome && parametro.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(nome);
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

        Gizmos.color = Color.red;
        Transform origem = pontoAtaque != null ? pontoAtaque : transform;
        Gizmos.DrawWireSphere(origem.position, raioAtaque);

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
