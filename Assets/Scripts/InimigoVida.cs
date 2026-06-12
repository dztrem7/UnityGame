using System.Collections;
using UnityEngine;

public class InimigoVida : MonoBehaviour
{
    public int vidaMaxima = 3;
    public float tempoParaSumirDepoisDaMorte = 1.2f;
    public float atrasoAntesDoFadeMorte = 0.35f;
    public float duracaoFadeMorte = 0.85f;
    public int piscadasMorte = 4;
    public bool piscarBrancoAoMorrer = true;
    public Color corPiscadaMorte = Color.white;
    [Range(0f, 1f)] public float forcaPiscadaMorte = 0.85f;
    public string triggerMorte = "Death";
    public string triggerDano = "Hit";
    public string estadoMorte = "Death";
    public string estadoDano = "Hit";
    public bool tocarEstadosDireto = true;

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioSource audioSourceEfeitos;
    public AudioClip somDano;
    public AudioClip somMorte;
    [Range(0f, 1f)] public float volumeDano = 0.85f;
    [Range(0f, 1f)] public float volumeMorte = 0.95f;
    public bool audio3D = true;
    public float distanciaMinimaAudio = 1.5f;
    public float distanciaMaximaAudio = 12f;

    private int vidaAtual;
    private Animator anim;
    private Collider2D[] colliders;
    private SpriteRenderer[] renderers;
    private Color[] coresOriginais;
    private Rigidbody2D rb;
    private bool morto;

    public bool EstaMorto => morto;

    private void Awake()
    {
        vidaAtual = vidaMaxima;
        anim = GetComponent<Animator>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        colliders = GetComponentsInChildren<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        coresOriginais = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            coresOriginais[i] = renderers[i] != null ? renderers[i].color : Color.white;

        rb = GetComponent<Rigidbody2D>();
        PreencherSonsNoEditor();
        ConfigurarAudioSource();

    }

    public void ReceberDano(int dano)
    {
        if (morto)
            return;

        vidaAtual -= Mathf.Max(1, dano);

        if (vidaAtual <= 0)
        {
            Morrer();
            return;
        }

        TocarSomDano();

        if (!SetTriggerSeExistir(triggerDano))
            TocarEstadoDireto(estadoDano);
    }

    private void Morrer()
    {
        morto = true;
        vidaAtual = 0;
        TocarSomMorte();

        LoboInimigo lobo = GetComponent<LoboInimigo>();
        if (lobo != null)
            lobo.enabled = false;

        InimigoContatoDano contatoDano = GetComponent<InimigoContatoDano>();
        if (contatoDano != null)
            contatoDano.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        if (!SetTriggerSeExistir(triggerMorte))
            TocarEstadoDireto(estadoMorte);
        StartCoroutine(SumirDepoisDaMorte());
    }

    private IEnumerator SumirDepoisDaMorte()
    {
        float atraso = Mathf.Max(0.35f, atrasoAntesDoFadeMorte);
        float duracaoFade = Mathf.Max(0.85f, duracaoFadeMorte);
        float tempoTotal = Mathf.Max(1.45f, tempoParaSumirDepoisDaMorte);

        yield return new WaitForSeconds(atraso);
        yield return StartCoroutine(PiscarEFazerFadeMorte());
        yield return new WaitForSeconds(Mathf.Max(0f, tempoTotal - atraso - duracaoFade));
        Destroy(gameObject);
    }

    private IEnumerator PiscarEFazerFadeMorte()
    {
        if (renderers == null || renderers.Length == 0)
            yield break;

        float tempo = 0f;
        float duracao = Mathf.Max(0.05f, duracaoFadeMorte);
        int piscadas = Mathf.Max(1, piscadasMorte);

        while (tempo < duracao)
        {
            float progresso = Mathf.Clamp01(tempo / duracao);
            float alphaFade = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, progresso));
            float pulso = Mathf.PingPong(progresso * piscadas * 2f, 1f);
            float alpha = alphaFade * Mathf.Lerp(0.25f, 1f, pulso);

            AplicarEfeitoMorte(alpha, pulso);

            tempo += Time.deltaTime;
            yield return null;
        }

        AplicarEfeitoMorte(0f, 0f);
    }

    private void AplicarEfeitoMorte(float alpha, float pulso)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Color cor = i < coresOriginais.Length ? coresOriginais[i] : renderers[i].color;
            if (piscarBrancoAoMorrer)
                cor = Color.Lerp(cor, corPiscadaMorte, pulso * forcaPiscadaMorte);

            cor.a = alpha;
            renderers[i].color = cor;
        }
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

    private void ConfigurarAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        AplicarConfiguracaoAudio3D(audioSource);

        if (audioSourceEfeitos == null)
            audioSourceEfeitos = gameObject.AddComponent<AudioSource>();

        audioSourceEfeitos.playOnAwake = false;
        audioSourceEfeitos.loop = false;
        AplicarConfiguracaoAudio3D(audioSourceEfeitos);
    }

    private void TocarSomDano()
    {
        if (audioSourceEfeitos == null || somDano == null)
            return;

        audioSourceEfeitos.PlayOneShot(somDano, volumeDano);
    }

    private void TocarSomMorte()
    {
        if (somMorte == null)
            return;

        GameObject somObj = new GameObject("SomMorteInimigo");
        somObj.transform.position = transform.position;

        AudioSource source = somObj.AddComponent<AudioSource>();
        AplicarConfiguracaoAudio3D(source);
        source.clip = somMorte;
        source.volume = volumeMorte;
        source.Play();

        Destroy(somObj, somMorte.length + 0.1f);
    }

    private void AplicarConfiguracaoAudio3D(AudioSource source)
    {
        if (source == null)
            return;

        source.spatialBlend = audio3D ? 1f : 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = Mathf.Max(0.1f, distanciaMinimaAudio);
        source.maxDistance = Mathf.Max(source.minDistance + 0.1f, distanciaMaximaAudio);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        PreencherSonsNoEditor();

        if (audioSource != null)
            AplicarConfiguracaoAudio3D(audioSource);

        if (audioSourceEfeitos != null)
            AplicarConfiguracaoAudio3D(audioSourceEfeitos);
    }

    private void PreencherSonsNoEditor()
    {
        bool ehLobo = GetComponent<LoboInimigo>() != null;
        bool ehIce = GetComponent<InimigoContatoDano>() != null;

        if (ehLobo && somDano == null)
            somDano = CarregarClipEditor("WolfDamaged");

        if (ehLobo && somMorte == null)
            somMorte = CarregarClipEditor("WolfDeath");

        if (ehIce && somDano == null)
            somDano = CarregarClipEditor("IceGolemDamaged");

        if (ehIce && somMorte == null)
            somMorte = CarregarClipEditor("IceGolemDeath");
    }

    private AudioClip CarregarClipEditor(string nome)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(nome + " t:AudioClip");

        foreach (string guid in guids)
        {
            string caminho = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            string nomePasta = nome.StartsWith("Wolf") ? "/Wolf/" : "/IceGolem/";

            if (caminho.Contains(nomePasta) && System.IO.Path.GetFileNameWithoutExtension(caminho) == nome)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(caminho);
        }

        return null;
    }
#else
    private void PreencherSonsNoEditor()
    {
    }
#endif

    private void TocarEstadoDireto(string nomeEstado)
    {
        if (!tocarEstadosDireto || anim == null || string.IsNullOrEmpty(nomeEstado))
            return;

        anim.Play(nomeEstado, 0, 0f);
    }
}
