using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private string nomeDoLevelDeJogo = "SampleScene";
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;

    [Header("Visual")]
    [SerializeField] private Color corTextoBotoes = Color.white;
    [SerializeField] private float duracaoEntradaMenu = 0.8f;
    [SerializeField] private float atrasoEntreBotoes = 0.1f;
    [SerializeField] private float antecipacaoVisualBotoes = 0.22f;
    [SerializeField] private float deslocamentoEntradaBotoes = 42f;

    [Header("Vinheta")]
    [SerializeField] private float alphaCentroVignette = 0.5f;
    [SerializeField] private float alphaBordaVignette = 0.96f;
    [SerializeField] private float forcaVignette = 1.9f;
    [SerializeField] private float duracaoFadeVignette = 1.6f;
    [SerializeField] private int resolucaoVignette = 256;

    [Header("Iris")]
    [SerializeField] private bool usarIrisEntrada = false;
    [SerializeField] private float duracaoIrisEntrada = 4f;
    [SerializeField] private float raioInicialIris = 0.06f;
    [SerializeField] private float raioFinalIris = 1.25f;
    [SerializeField] private float suavidadeBordaIris = 0.08f;
    [SerializeField] private float intensidadeManchaIris = 0.04f;
    [SerializeField] private float escalaManchaIris = 10f;
    [SerializeField] private float velocidadeManchaIris = 0.7f;
    [SerializeField] private float atrasoBotoesAposIris = 0.45f;
    [SerializeField] private Color corBordaGosmaIris = Color.black;
    [SerializeField] private float intensidadeBordaGosmaIris = 0f;
    [SerializeField] private float alphaPretoIris = 1f;
    [SerializeField] private int resolucaoIris = 256;

    [Header("Logo Studio")]
    [SerializeField] private bool mostrarLogoStudio = true;
    [SerializeField] private string nomeTexturaLogoStudio = "DreamcoreStudioLogoRecorte";
    [SerializeField] private Vector2 tamanhoLogoStudio = new Vector2(520f, 520f);
    [SerializeField] private float duracaoFadeEntradaLogo = 1.8f;
    [SerializeField] private float tempoLogoVisivel = 1.2f;
    [SerializeField] private float duracaoFadeSaidaLogo = 1.8f;

    [Header("Selecao")]
    [SerializeField] private float distanciaSetasSelecao = 110f;
    [SerializeField] private string textoSetaEsquerda = "<";
    [SerializeField] private string textoSetaDireita = ">";
    [SerializeField] private bool esconderSetasNoMobile = true;
    [SerializeField] private bool detectarMobilePorAspectoNoEditor = true;
    [SerializeField] private float aspectoMinimoCelularWide = 1.85f;
    [SerializeField] private float aspectoMaximoTablet = 1.55f;

    [Header("FPS")]
    [SerializeField] private bool criarOpcoesFPSAutomaticamente = true;
    [SerializeField] private Vector2 posicaoGrupoFPS = new Vector2(0f, -170f);
    [SerializeField] private float espacamentoBotoesFPS = 135f;
    [SerializeField] private float tamanhoTextoFPS = 28f;
    [SerializeField] private Color corFPSSelecionado = new Color(0.72f, 0.9f, 1f, 1f);
    [SerializeField] private Color corFPSNormal = Color.white;

    private readonly List<BotaoMenuAnimado> botoesAtivos = new List<BotaoMenuAnimado>();
    private readonly List<ElementoOpcaoAnimado> elementosOpcoesAtivos = new List<ElementoOpcaoAnimado>();
    private Canvas canvas;
    private Canvas canvasEfeitos;
    private RawImage fundoVignette;
    private RawImage irisEntrada;
    private CanvasGroup grupoLogoStudio;
    private Image fundoLogoStudio;
    private RawImage imagemLogoStudio;
    private Texture2D texturaIris;
    private Color32[] pixelsIris;
    private RectTransform setaEsquerda;
    private RectTransform setaDireita;
    private Text textoSetaEsquerdaUI;
    private Text textoSetaDireitaUI;
    private readonly List<Text> textosBotoesFPS = new List<Text>();
    private readonly List<TMP_Text> textosTMPBotoesFPS = new List<TMP_Text>();
    private int indiceSelecionado = 0;
    private Coroutine animacaoBotoesCoroutine;
    private Coroutine animacaoElementosOpcoesCoroutine;

    private void Start()
    {
        PrepararVisualMenu();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        CriarOpcoesFPSSePreciso();
        AtualizarVisualBotoesFPS();
    }
#endif

    private void Update()
    {
        if (botoesAtivos.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            MudarSelecao(1);

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            MudarSelecao(-1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            AtivarBotaoSelecionado();

        AtualizarSetasSelecao();
    }

    public void Jogar()
    {
        SceneManager.LoadScene(nomeDoLevelDeJogo);
    }

    public void IrParaMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void AbrirOpcoes()
    {
        if (painelMenuInicial != null)
            painelMenuInicial.SetActive(false);

        if (painelOpcoes != null)
            painelOpcoes.SetActive(true);

        PrepararBotoesAtivos();
        PrepararElementosOpcoesAtivos();
        AtualizarVisualBotoesFPS();
        TocarAnimacaoBotoes();
        TocarAnimacaoElementosOpcoes();
    }

    public void FecharOpcoes()
    {
        if (painelOpcoes != null)
            painelOpcoes.SetActive(false);

        if (painelMenuInicial != null)
            painelMenuInicial.SetActive(true);

        PrepararBotoesAtivos();
        TocarAnimacaoBotoes();
    }

    public void SairJogo()
    {
        Debug.Log("Sair do Jogo");
        Application.Quit();
    }

    public void DefinirFPS30()
    {
        DefinirFPS(30);
    }

    public void DefinirFPS60()
    {
        DefinirFPS(60);
    }

    public void DefinirFPS120()
    {
        DefinirFPS(120);
    }

    private void DefinirFPS(int fps)
    {
        ConfiguracaoFPS.AplicarFPS(fps, true);
        AtualizarVisualBotoesFPS();
        PrepararBotoesAtivos();
        AtualizarSetasSelecao();
    }

    private void PrepararVisualMenu()
    {
        canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        PrepararCanvasDeEfeitos();
        CriarVignetteSePreciso();
        if (usarIrisEntrada)
            CriarIrisEntradaSePreciso();
        else
            DesativarObjetoVisualAntigo("IrisGosmentoMenu");
        CriarLogoStudioSePreciso();
        CriarSetasSelecaoSePreciso();
        CriarOpcoesFPSSePreciso();
        AtualizarVisualBotoesFPS();
        PrepararBotoesAtivos();

        StartCoroutine(TocarEntradaMenu());
    }

    private void CriarVignetteSePreciso()
    {
        if (fundoVignette != null)
            return;

        Transform fundoExistente = canvasEfeitos.transform.Find("FundoVignetteMenu");
        GameObject objetoFundo = fundoExistente != null ? fundoExistente.gameObject : new GameObject("FundoVignetteMenu");
        objetoFundo.transform.SetParent(canvasEfeitos.transform, false);

        fundoVignette = objetoFundo.GetComponent<RawImage>();

        if (fundoVignette == null)
            fundoVignette = objetoFundo.AddComponent<RawImage>();

        fundoVignette.raycastTarget = false;

        RectTransform rectTransform = fundoVignette.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Texture2D textura = new Texture2D(resolucaoVignette, resolucaoVignette, TextureFormat.RGBA32, false);
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[resolucaoVignette * resolucaoVignette];
        int index = 0;

        for (int y = 0; y < resolucaoVignette; y++)
        {
            float v = (float)y / (resolucaoVignette - 1);

            for (int x = 0; x < resolucaoVignette; x++)
            {
                float u = (float)x / (resolucaoVignette - 1);
                float dx = u - 0.5f;
                float dy = v - 0.5f;
                float distancia = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) * 1.45f);
                float alpha = Mathf.Lerp(alphaCentroVignette, alphaBordaVignette, Mathf.Pow(distancia, forcaVignette));

                pixels[index] = new Color32(0, 0, 0, (byte)(Mathf.Clamp01(alpha) * 255));
                index++;
            }
        }

        textura.SetPixels32(pixels);
        textura.Apply(false);

        fundoVignette.texture = textura;
        fundoVignette.color = new Color(1f, 1f, 1f, 0f);
        fundoVignette.transform.SetAsLastSibling();
    }

    private void PrepararCanvasDeEfeitos()
    {
        if (canvasEfeitos != null)
            return;

        canvas.overrideSorting = true;
        canvas.sortingOrder = 10;

        GameObject canvasExistente = GameObject.Find("CanvasEfeitosMenu");

        if (canvasExistente != null)
        {
            canvasEfeitos = canvasExistente.GetComponent<Canvas>();

            if (canvasEfeitos != null)
            {
                canvasEfeitos.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasEfeitos.overrideSorting = true;
                canvasEfeitos.sortingOrder = 5;
                NormalizarCanvasEfeitos();
                return;
            }
        }

        GameObject objetoCanvas = new GameObject("CanvasEfeitosMenu");
        canvasEfeitos = objetoCanvas.AddComponent<Canvas>();
        canvasEfeitos.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasEfeitos.overrideSorting = true;
        canvasEfeitos.sortingOrder = 5;

        CanvasScaler scaler = objetoCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        objetoCanvas.AddComponent<GraphicRaycaster>();
        NormalizarCanvasEfeitos();
    }

    private void NormalizarCanvasEfeitos()
    {
        if (canvasEfeitos == null)
            return;

        RectTransform rectTransform = canvasEfeitos.GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        rectTransform.localPosition = Vector3.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    private void DesativarObjetoVisualAntigo(string nome)
    {
        Transform[] objetos = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform objeto in objetos)
        {
            if (objeto.name == nome && objeto.gameObject.scene.IsValid())
                objeto.gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimarVignetteEntrada()
    {
        if (fundoVignette == null)
            yield break;

        float tempo = 0f;

        while (tempo < duracaoFadeVignette)
        {
            float progresso = Mathf.SmoothStep(0f, 1f, tempo / duracaoFadeVignette);
            fundoVignette.color = new Color(1f, 1f, 1f, progresso);
            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        fundoVignette.color = Color.white;
    }

    private IEnumerator TocarEntradaMenu()
    {
        OcultarBotoesMenu();
        AplicarAlphaSetas(0f);

        if (irisEntrada != null)
            irisEntrada.gameObject.SetActive(false);

        if (mostrarLogoStudio && grupoLogoStudio != null)
            yield return StartCoroutine(TocarIntroLogoStudio());

        if (fundoVignette != null)
            StartCoroutine(AnimarVignetteEntrada());

        if (usarIrisEntrada && irisEntrada != null)
            yield return StartCoroutine(AnimarIrisEntrada());

        MostrarBotoesSemAnimacao();
    }

    private void CriarLogoStudioSePreciso()
    {
        if (!mostrarLogoStudio || grupoLogoStudio != null || canvasEfeitos == null)
            return;

        Texture2D texturaLogo = Resources.Load<Texture2D>(nomeTexturaLogoStudio);

        if (texturaLogo == null)
            return;

        Transform existente = canvasEfeitos.transform.Find("IntroLogoStudio");
        GameObject objetoLogo = existente != null ? existente.gameObject : new GameObject("IntroLogoStudio");
        objetoLogo.transform.SetParent(canvasEfeitos.transform, false);

        grupoLogoStudio = objetoLogo.GetComponent<CanvasGroup>();

        if (grupoLogoStudio == null)
            grupoLogoStudio = objetoLogo.AddComponent<CanvasGroup>();

        grupoLogoStudio.alpha = 0f;
        grupoLogoStudio.blocksRaycasts = true;
        grupoLogoStudio.interactable = false;

        RectTransform rectLogoRoot = objetoLogo.GetComponent<RectTransform>();

        if (rectLogoRoot == null)
            rectLogoRoot = objetoLogo.AddComponent<RectTransform>();

        rectLogoRoot.anchorMin = Vector2.zero;
        rectLogoRoot.anchorMax = Vector2.one;
        rectLogoRoot.offsetMin = Vector2.zero;
        rectLogoRoot.offsetMax = Vector2.zero;
        rectLogoRoot.pivot = new Vector2(0.5f, 0.5f);

        fundoLogoStudio = CriarImagemFundoLogo(objetoLogo.transform);
        imagemLogoStudio = CriarImagemLogo(objetoLogo.transform, texturaLogo);
        objetoLogo.transform.SetAsLastSibling();
        objetoLogo.SetActive(false);
    }

    private Image CriarImagemFundoLogo(Transform pai)
    {
        Transform existente = pai.Find("FundoPretoLogo");
        GameObject objetoFundo = existente != null ? existente.gameObject : new GameObject("FundoPretoLogo");
        objetoFundo.transform.SetParent(pai, false);

        Image imagem = objetoFundo.GetComponent<Image>();

        if (imagem == null)
            imagem = objetoFundo.AddComponent<Image>();

        imagem.color = Color.black;
        imagem.raycastTarget = false;

        RectTransform rectTransform = imagem.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return imagem;
    }

    private RawImage CriarImagemLogo(Transform pai, Texture2D texturaLogo)
    {
        Transform existente = pai.Find("LogoDreamcoreStudio");
        GameObject objetoImagem = existente != null ? existente.gameObject : new GameObject("LogoDreamcoreStudio");
        objetoImagem.transform.SetParent(pai, false);

        RawImage imagem = objetoImagem.GetComponent<RawImage>();

        if (imagem == null)
            imagem = objetoImagem.AddComponent<RawImage>();

        imagem.texture = texturaLogo;
        imagem.color = Color.white;
        imagem.raycastTarget = false;

        RectTransform rectTransform = imagem.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = tamanhoLogoStudio;

        return imagem;
    }

    private IEnumerator TocarIntroLogoStudio()
    {
        canvasEfeitos.sortingOrder = 80;
        grupoLogoStudio.gameObject.SetActive(true);
        grupoLogoStudio.transform.SetAsLastSibling();
        grupoLogoStudio.alpha = 1f;
        AplicarAlphaImagemLogo(0f);

        yield return StartCoroutine(AnimarAlphaLogo(0f, 1f, duracaoFadeEntradaLogo));
        yield return new WaitForSecondsRealtime(tempoLogoVisivel);
        yield return StartCoroutine(AnimarAlphaLogo(1f, 0f, duracaoFadeSaidaLogo));

        grupoLogoStudio.gameObject.SetActive(false);
        canvasEfeitos.sortingOrder = 5;
    }

    private IEnumerator AnimarAlphaLogo(float inicio, float fim, float duracao)
    {
        float tempo = 0f;
        duracao = Mathf.Max(0.01f, duracao);

        while (tempo < duracao)
        {
            float progresso = Mathf.SmoothStep(0f, 1f, tempo / duracao);
            AplicarAlphaImagemLogo(Mathf.Lerp(inicio, fim, progresso));
            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        AplicarAlphaImagemLogo(fim);
    }

    private void AplicarAlphaImagemLogo(float alpha)
    {
        if (imagemLogoStudio == null)
            return;

        Color cor = imagemLogoStudio.color;
        cor.a = alpha;
        imagemLogoStudio.color = cor;
    }

    private void OcultarBotoesMenu()
    {
        foreach (BotaoMenuAnimado botao in botoesAtivos)
        {
            if (botao.rectTransform != null)
                botao.rectTransform.anchoredPosition = botao.posicaoOriginal;

            if (botao.botao != null)
                botao.botao.interactable = false;

            if (botao.canvasGroup != null)
                botao.canvasGroup.alpha = 0f;
        }
    }

    private void MostrarBotoesSemAnimacao()
    {
        foreach (BotaoMenuAnimado botao in botoesAtivos)
        {
            if (botao.rectTransform != null)
                botao.rectTransform.anchoredPosition = botao.posicaoOriginal;

            if (botao.botao != null)
                botao.botao.interactable = true;

            if (botao.canvasGroup != null)
                botao.canvasGroup.alpha = 1f;
        }

        AplicarAlphaSetas(1f);
    }

    private void PrepararBotoesParaIris()
    {
        foreach (BotaoMenuAnimado botao in botoesAtivos)
        {
            if (botao.rectTransform != null)
                botao.rectTransform.anchoredPosition = botao.posicaoOriginal;

            if (botao.canvasGroup != null)
                botao.canvasGroup.alpha = 1f;
        }
    }

    private void PrepararBotoesParaEntrada()
    {
        foreach (BotaoMenuAnimado botao in botoesAtivos)
            AplicarAnimacaoBotao(botao, 0f);
    }

    private void CriarIrisEntradaSePreciso()
    {
        if (irisEntrada != null)
            return;

        Transform irisExistente = canvasEfeitos.transform.Find("IrisGosmentoMenu");
        GameObject objetoIris = irisExistente != null ? irisExistente.gameObject : new GameObject("IrisGosmentoMenu");
        objetoIris.transform.SetParent(canvasEfeitos.transform, false);

        irisEntrada = objetoIris.GetComponent<RawImage>();

        if (irisEntrada == null)
            irisEntrada = objetoIris.AddComponent<RawImage>();

        irisEntrada.raycastTarget = false;

        RectTransform rectTransform = irisEntrada.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        texturaIris = new Texture2D(resolucaoIris, resolucaoIris, TextureFormat.RGBA32, false);
        texturaIris.wrapMode = TextureWrapMode.Clamp;
        texturaIris.filterMode = FilterMode.Bilinear;
        pixelsIris = new Color32[resolucaoIris * resolucaoIris];

        irisEntrada.texture = texturaIris;
        irisEntrada.color = Color.white;
        irisEntrada.transform.SetAsLastSibling();
    }

    private IEnumerator AnimarIrisEntrada()
    {
        canvasEfeitos.sortingOrder = 50;
        irisEntrada.gameObject.SetActive(true);
        irisEntrada.transform.SetAsLastSibling();

        float tempo = 0f;

        while (tempo < duracaoIrisEntrada)
        {
            float progresso = Mathf.SmoothStep(0f, 1f, tempo / duracaoIrisEntrada);
            float raio = Mathf.Lerp(raioInicialIris, raioFinalIris, progresso);

            AtualizarMascaraIris(raio, tempo);

            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        AtualizarMascaraIris(raioFinalIris, duracaoIrisEntrada);
        yield return new WaitForSecondsRealtime(atrasoBotoesAposIris);
        irisEntrada.gameObject.SetActive(false);
        canvasEfeitos.sortingOrder = 5;
    }

    private void AtualizarMascaraIris(float raio, float tempo)
    {
        Vector2 centro = new Vector2(0.5f, 0.5f);
        int index = 0;

        for (int y = 0; y < resolucaoIris; y++)
        {
            float v = (float)y / (resolucaoIris - 1);

            for (int x = 0; x < resolucaoIris; x++)
            {
                float u = (float)x / (resolucaoIris - 1);
                float distanciaX = u - centro.x;
                float distanciaY = v - centro.y;
                float distancia = Mathf.Sqrt(distanciaX * distanciaX + distanciaY * distanciaY);
                float mancha = CalcularManchaIris(u, v, tempo);
                float raioComMancha = raio + mancha * intensidadeManchaIris;
                float alpha = Mathf.InverseLerp(raioComMancha - suavidadeBordaIris, raioComMancha, distancia);
                alpha = Mathf.SmoothStep(0f, 1f, alpha);
                float borda = 1f - Mathf.Clamp01(Mathf.Abs(distancia - raioComMancha) / suavidadeBordaIris);
                borda = Mathf.SmoothStep(0f, 1f, borda) * intensidadeBordaGosmaIris;
                byte vermelho = (byte)(corBordaGosmaIris.r * borda * 255);
                byte verde = (byte)(corBordaGosmaIris.g * borda * 255);
                byte azul = (byte)(corBordaGosmaIris.b * borda * 255);

                pixelsIris[index] = new Color32(vermelho, verde, azul, (byte)(Mathf.Clamp01(alpha * alphaPretoIris) * 255));
                index++;
            }
        }

        texturaIris.SetPixels32(pixelsIris);
        texturaIris.Apply(false);
    }

    private float CalcularManchaIris(float u, float v, float tempo)
    {
        float movimento = tempo * velocidadeManchaIris;
        float ruidoGrande = Mathf.PerlinNoise(u * escalaManchaIris + movimento, v * escalaManchaIris - movimento * 0.35f);
        float ruidoPequeno = Mathf.PerlinNoise((u + 4.7f) * escalaManchaIris * 2.2f - movimento * 0.8f, (v + 9.1f) * escalaManchaIris * 2.2f + movimento);
        float fio = Mathf.PerlinNoise((u + 12.1f) * escalaManchaIris * 4.6f, (v + 2.9f) * escalaManchaIris * 4.6f + movimento * 1.3f);

        return ((ruidoGrande * 0.62f) + (ruidoPequeno * 0.28f) + (fio * 0.1f) - 0.5f) * 2f;
    }

    private void PrepararBotoesAtivos()
    {
        botoesAtivos.Clear();

        GameObject painelAtual = painelOpcoes != null && painelOpcoes.activeInHierarchy ? painelOpcoes : painelMenuInicial;

        if (painelAtual == null)
            return;

        Button[] botoes = painelAtual.GetComponentsInChildren<Button>(true);

        foreach (Button botao in botoes)
            botoesAtivos.Add(new BotaoMenuAnimado(botao, corTextoBotoes));

        indiceSelecionado = Mathf.Clamp(indiceSelecionado, 0, Mathf.Max(0, botoesAtivos.Count - 1));
        AtualizarSetasSelecao();
    }

    private void CriarOpcoesFPSSePreciso()
    {
        if (!criarOpcoesFPSAutomaticamente || painelOpcoes == null)
            return;

        Transform existente = painelOpcoes.transform.Find("OpcoesFPS");

        if (existente != null)
        {
            ConfigurarBotoesFPSExistentes(existente);
            RegistrarTextosFPS(existente);
            return;
        }

        GameObject grupo = new GameObject("OpcoesFPS");
        grupo.transform.SetParent(painelOpcoes.transform, false);

        RectTransform grupoRect = grupo.AddComponent<RectTransform>();
        grupoRect.anchorMin = new Vector2(0.5f, 0.5f);
        grupoRect.anchorMax = new Vector2(0.5f, 0.5f);
        grupoRect.pivot = new Vector2(0.5f, 0.5f);
        grupoRect.anchoredPosition = posicaoGrupoFPS;
        grupoRect.sizeDelta = new Vector2(560f, 120f);

        CriarTextoFPS(grupoRect, "TituloFPS", "FPS", new Vector2(0f, 36f), 34f, null);
        CriarBotaoFPS(grupoRect, "FPS30", "30", new Vector2(-espacamentoBotoesFPS, -18f), DefinirFPS30);
        CriarBotaoFPS(grupoRect, "FPS60", "60", new Vector2(0f, -18f), DefinirFPS60);
        CriarBotaoFPS(grupoRect, "FPS120", "120", new Vector2(espacamentoBotoesFPS, -18f), DefinirFPS120);
    }

    private void ConfigurarBotoesFPSExistentes(Transform grupo)
    {
        ConfigurarBotaoFPSExistente(grupo, "FPS30", DefinirFPS30);
        ConfigurarBotaoFPSExistente(grupo, "FPS60", DefinirFPS60);
        ConfigurarBotaoFPSExistente(grupo, "FPS120", DefinirFPS120);
    }

    private void ConfigurarBotaoFPSExistente(Transform grupo, string nome, UnityEngine.Events.UnityAction acao)
    {
        Transform encontrado = grupo.Find(nome);

        if (encontrado == null)
            return;

        Button botao = encontrado.GetComponent<Button>();

        if (botao == null)
            botao = encontrado.gameObject.AddComponent<Button>();

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(acao);
    }

    private void CriarBotaoFPS(RectTransform pai, string nome, string texto, Vector2 posicao, UnityEngine.Events.UnityAction acao)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(pai, false);

        RectTransform rect = objeto.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicao;
        rect.sizeDelta = new Vector2(110f, 54f);

        Image imagem = objeto.AddComponent<Image>();
        imagem.color = new Color(1f, 1f, 1f, 0f);

        Button botao = objeto.AddComponent<Button>();
        botao.transition = Selectable.Transition.ColorTint;
        botao.onClick.AddListener(acao);

        CriarTextoFPS(rect, "Text", texto, Vector2.zero, tamanhoTextoFPS, botao);
    }

    private void CriarTextoFPS(RectTransform pai, string nome, string texto, Vector2 posicao, float tamanho, Button botao)
    {
        GameObject objetoTexto = new GameObject(nome);
        objetoTexto.transform.SetParent(pai, false);

        RectTransform rectTexto = objetoTexto.AddComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
        rectTexto.anchoredPosition = posicao;

        Text textoUI = objetoTexto.AddComponent<Text>();
        textoUI.text = texto;
        textoUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoUI.fontSize = Mathf.RoundToInt(tamanho);
        textoUI.fontStyle = FontStyle.Bold;
        textoUI.alignment = TextAnchor.MiddleCenter;
        textoUI.color = corFPSNormal;
        textoUI.raycastTarget = false;

        if (botao != null)
            textosBotoesFPS.Add(textoUI);
    }

    private void RegistrarTextosFPS(Transform grupo)
    {
        textosBotoesFPS.Clear();
        textosTMPBotoesFPS.Clear();

        foreach (Button botao in grupo.GetComponentsInChildren<Button>(true))
        {
            Text texto = botao.GetComponentInChildren<Text>(true);
            if (texto != null)
                textosBotoesFPS.Add(texto);

            TMP_Text textoTMP = botao.GetComponentInChildren<TMP_Text>(true);
            if (textoTMP != null)
                textosTMPBotoesFPS.Add(textoTMP);
        }
    }

    private void AtualizarVisualBotoesFPS()
    {
        int fpsAtual = ConfiguracaoFPS.FPSAtualSalvo();

        foreach (Text texto in textosBotoesFPS)
        {
            if (texto == null)
                continue;

            texto.color = texto.text == fpsAtual.ToString() ? corFPSSelecionado : corFPSNormal;
        }

        foreach (TMP_Text texto in textosTMPBotoesFPS)
        {
            if (texto == null)
                continue;

            texto.color = texto.text == fpsAtual.ToString() ? corFPSSelecionado : corFPSNormal;
        }
    }

    private void TocarAnimacaoBotoes()
    {
        if (animacaoBotoesCoroutine != null)
            StopCoroutine(animacaoBotoesCoroutine);

        animacaoBotoesCoroutine = StartCoroutine(AnimarBotoes());
    }

    private IEnumerator AnimarBotoesDepoisDaIris()
    {
        float tempo = 0f;

        while (tempo < duracaoEntradaMenu)
        {
            float progresso = Mathf.SmoothStep(0f, 1f, tempo / duracaoEntradaMenu);
            float deslocamento = Mathf.Lerp(deslocamentoEntradaBotoes * 0.45f, 0f, progresso);

            foreach (BotaoMenuAnimado botao in botoesAtivos)
            {
                if (botao.rectTransform != null)
                    botao.rectTransform.anchoredPosition = botao.posicaoOriginal + Vector2.down * deslocamento;

                if (botao.canvasGroup != null)
                    botao.canvasGroup.alpha = 1f;
            }

            AplicarAlphaSetas(progresso);
            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach (BotaoMenuAnimado botao in botoesAtivos)
            AplicarAnimacaoBotao(botao, 1f);

        AplicarAlphaSetas(1f);
    }

    private IEnumerator AnimarBotoes()
    {
        float tempo = 0f;
        float duracaoTotal = duracaoEntradaMenu + atrasoEntreBotoes * Mathf.Max(0, botoesAtivos.Count - 1);

        while (tempo < duracaoTotal)
        {
            for (int i = 0; i < botoesAtivos.Count; i++)
            {
                float tempoBotao = tempo + duracaoEntradaMenu * antecipacaoVisualBotoes - atrasoEntreBotoes * i;
                float progresso = Mathf.Clamp01(tempoBotao / duracaoEntradaMenu);
                AplicarAnimacaoBotao(botoesAtivos[i], progresso);
            }

            AplicarAlphaSetas(Mathf.Clamp01(tempo / duracaoEntradaMenu));
            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach (BotaoMenuAnimado botao in botoesAtivos)
            AplicarAnimacaoBotao(botao, 1f);

        AplicarAlphaSetas(1f);
    }

    private void PrepararElementosOpcoesAtivos()
    {
        elementosOpcoesAtivos.Clear();

        if (painelOpcoes == null || !painelOpcoes.activeInHierarchy)
            return;

        RectTransform[] elementos = painelOpcoes.GetComponentsInChildren<RectTransform>(true);

        foreach (RectTransform elemento in elementos)
        {
            if (elemento == null || elemento.gameObject == painelOpcoes)
                continue;

            if (elemento.GetComponentInParent<Button>(true) != null)
                continue;

            Slider sliderPai = elemento.GetComponentInParent<Slider>(true);
            if (sliderPai != null && sliderPai.transform != elemento)
                continue;

            bool animavel = elemento.GetComponent<TMP_Text>() != null
                || elemento.GetComponent<Text>() != null
                || elemento.GetComponent<Slider>() != null;

            if (animavel)
                elementosOpcoesAtivos.Add(new ElementoOpcaoAnimado(elemento));
        }
    }

    private void TocarAnimacaoElementosOpcoes()
    {
        if (animacaoElementosOpcoesCoroutine != null)
            StopCoroutine(animacaoElementosOpcoesCoroutine);

        animacaoElementosOpcoesCoroutine = StartCoroutine(AnimarElementosOpcoes());
    }

    private IEnumerator AnimarElementosOpcoes()
    {
        float tempo = 0f;

        foreach (ElementoOpcaoAnimado elemento in elementosOpcoesAtivos)
            AplicarAnimacaoElementoOpcao(elemento, 0f);

        while (tempo < duracaoEntradaMenu)
        {
            float progresso = Mathf.Clamp01(tempo / duracaoEntradaMenu);

            foreach (ElementoOpcaoAnimado elemento in elementosOpcoesAtivos)
                AplicarAnimacaoElementoOpcao(elemento, progresso);

            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach (ElementoOpcaoAnimado elemento in elementosOpcoesAtivos)
            AplicarAnimacaoElementoOpcao(elemento, 1f);
    }

    private void AplicarAnimacaoElementoOpcao(ElementoOpcaoAnimado elemento, float progresso)
    {
        if (elemento == null || elemento.rectTransform == null)
            return;

        float suavizado = Mathf.SmoothStep(0f, 1f, progresso);
        float deslocamento = Mathf.Lerp(deslocamentoEntradaBotoes * 0.65f, 0f, suavizado);

        elemento.rectTransform.anchoredPosition = elemento.posicaoOriginal + Vector2.down * deslocamento;

        if (elemento.canvasGroup != null)
            elemento.canvasGroup.alpha = suavizado;
    }

    private void AplicarAnimacaoBotao(BotaoMenuAnimado botao, float progresso)
    {
        float suavizado = Mathf.SmoothStep(0f, 1f, progresso);
        float deslocamento = Mathf.Lerp(deslocamentoEntradaBotoes, 0f, suavizado);

        if (botao.rectTransform != null)
            botao.rectTransform.anchoredPosition = botao.posicaoOriginal + Vector2.down * deslocamento;

        if (botao.canvasGroup != null)
            botao.canvasGroup.alpha = suavizado;
    }

    private void CriarSetasSelecaoSePreciso()
    {
        if (setaEsquerda != null || canvas == null)
            return;

        setaEsquerda = CriarSetaSelecao("SetaMenuEsquerda", textoSetaEsquerda, out textoSetaEsquerdaUI);
        setaDireita = CriarSetaSelecao("SetaMenuDireita", textoSetaDireita, out textoSetaDireitaUI);
    }

    private RectTransform CriarSetaSelecao(string nome, string textoSeta, out Text textoUI)
    {
        GameObject objetoSeta = new GameObject(nome);
        objetoSeta.transform.SetParent(canvas.transform, false);

        CanvasGroup grupo = objetoSeta.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;

        textoUI = objetoSeta.AddComponent<Text>();
        textoUI.text = textoSeta;
        textoUI.color = corTextoBotoes;
        textoUI.alignment = TextAnchor.MiddleCenter;
        textoUI.raycastTarget = false;
        textoUI.fontSize = 34;
        textoUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform rectTransform = objetoSeta.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(42f, 42f);

        return rectTransform;
    }

    private void MudarSelecao(int direcao)
    {
        indiceSelecionado += direcao;

        if (indiceSelecionado < 0)
            indiceSelecionado = botoesAtivos.Count - 1;
        else if (indiceSelecionado >= botoesAtivos.Count)
            indiceSelecionado = 0;

        AtualizarSetasSelecao();
    }

    private void AtivarBotaoSelecionado()
    {
        if (indiceSelecionado < 0 || indiceSelecionado >= botoesAtivos.Count)
            return;

        botoesAtivos[indiceSelecionado].botao.onClick.Invoke();
    }

    private void AtualizarSetasSelecao()
    {
        if (setaEsquerda == null || setaDireita == null || botoesAtivos.Count == 0)
            return;

        bool mostrarSetas = !(esconderSetasNoMobile && EhLayoutMobile());
        setaEsquerda.gameObject.SetActive(mostrarSetas);
        setaDireita.gameObject.SetActive(mostrarSetas);

        if (!mostrarSetas)
            return;

        BotaoMenuAnimado selecionado = botoesAtivos[Mathf.Clamp(indiceSelecionado, 0, botoesAtivos.Count - 1)];

        if (selecionado.rectTransform == null)
            return;

        Vector3 posicaoMundo = selecionado.rectTransform.TransformPoint(selecionado.rectTransform.rect.center);
        Vector2 posicaoTela = RectTransformUtility.WorldToScreenPoint(null, posicaoMundo);
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, posicaoTela, null, out Vector2 posicaoLocal))
            return;

        setaEsquerda.anchoredPosition = posicaoLocal + Vector2.left * distanciaSetasSelecao;
        setaDireita.anchoredPosition = posicaoLocal + Vector2.right * distanciaSetasSelecao;
        setaEsquerda.SetAsLastSibling();
        setaDireita.SetAsLastSibling();
    }

    private void AplicarAlphaSetas(float progresso)
    {
        if (esconderSetasNoMobile && EhLayoutMobile())
        {
            if (setaEsquerda != null)
                setaEsquerda.gameObject.SetActive(false);

            if (setaDireita != null)
                setaDireita.gameObject.SetActive(false);

            return;
        }

        float suavizado = Mathf.SmoothStep(0f, 1f, progresso);

        if (setaEsquerda != null && setaEsquerda.TryGetComponent(out CanvasGroup grupoEsquerda))
            grupoEsquerda.alpha = suavizado;

        if (setaDireita != null && setaDireita.TryGetComponent(out CanvasGroup grupoDireita))
            grupoDireita.alpha = suavizado;
    }

    private bool EhLayoutMobile()
    {
        if (Application.isMobilePlatform)
            return true;

        if (Screen.width <= 0 || Screen.height <= 0)
            return false;

        float aspecto = (float)Screen.width / Screen.height;
        Rect safeArea = Screen.safeArea;
        bool temSafeAreaDiferente = safeArea.xMin > 1f
            || safeArea.yMin > 1f
            || safeArea.xMax < Screen.width - 1f
            || safeArea.yMax < Screen.height - 1f;

        bool aspectoMobile = detectarMobilePorAspectoNoEditor
            && (aspecto >= aspectoMinimoCelularWide || aspecto <= aspectoMaximoTablet);

        return temSafeAreaDiferente || aspectoMobile;
    }

    private class BotaoMenuAnimado
    {
        public Button botao;
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public Vector2 posicaoOriginal;

        public BotaoMenuAnimado(Button botao, Color corTexto)
        {
            this.botao = botao;
            rectTransform = botao.GetComponent<RectTransform>();
            canvasGroup = botao.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = botao.gameObject.AddComponent<CanvasGroup>();

            Image imagem = botao.GetComponent<Image>();

            if (imagem != null)
                imagem.color = new Color(1f, 1f, 1f, 0f);

            TMP_Text textoTMP = botao.GetComponentInChildren<TMP_Text>(true);

            if (textoTMP != null)
            {
                textoTMP.color = corTexto;
                textoTMP.alignment = TextAlignmentOptions.Center;
            }

            Text texto = botao.GetComponentInChildren<Text>(true);

            if (texto != null)
            {
                texto.color = corTexto;
                texto.alignment = TextAnchor.MiddleCenter;
            }

            if (rectTransform != null)
                posicaoOriginal = rectTransform.anchoredPosition;
        }
    }

    private class ElementoOpcaoAnimado
    {
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public Vector2 posicaoOriginal;

        public ElementoOpcaoAnimado(RectTransform rectTransform)
        {
            this.rectTransform = rectTransform;
            canvasGroup = rectTransform.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();

            posicaoOriginal = rectTransform.anchoredPosition;
        }
    }
}
