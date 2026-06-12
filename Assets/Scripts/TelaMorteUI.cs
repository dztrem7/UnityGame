using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TelaMorteUI : MonoBehaviour
{
    public GameObject painelMorte;
    public GameObject botaoPause;
    public Button botaoReiniciar;
    public Button botaoMenu;
    public string nomePainelMorte = "PainelMorte";
    public Color corFundoPainelMorte = Color.black;
    public string nomeCenaMenu = "TelaInicial";
    public float atrasoParaMostrar = 0.2f;
    public bool pausarJogoAoMostrar = true;
    public bool usarTransicaoIris = true;
    public float duracaoTransicaoIris = 2.2f;
    public float raioInicialIris = 1.1f;
    public float raioFinalIris = 0.04f;
    public float suavidadeBordaIris = 0.16f;
    public float intensidadeManchaIris = 0.12f;
    public float escalaManchaIris = 18f;
    public int resolucaoMascaraIris = 256;
    public Color corTextoBotoes = Color.white;
    public float duracaoEntradaBotoes = 0.65f;
    public float atrasoEntreBotoes = 0.08f;
    public float deslocamentoEntradaBotoes = 18f;
    public float velocidadeBrilhoBotoes = 2.2f;
    public float intensidadeBrilhoBotoes = 0.22f;
    public float distanciaSetasSelecao = 95f;
    public float tamanhoTextoBotoes = 18f;
    public float tamanhoTextoBotoesMobile = 58f;
    public int tamanhoTextoSetas = 34;
    public int tamanhoTextoSetasMobile = 66;
    public float espacamentoBotoesMobile = 135f;
    public bool forcarLayoutMobileNoEditor = false;
    public bool detectarMobilePorSafeAreaNoEditor = true;
    public bool detectarMobilePorAspectoNoEditor = false;
    public float aspectoMinimoCelularWide = 1.85f;
    public float aspectoMaximoTablet = 1.55f;
    public string textoSetaEsquerda = "<";
    public string textoSetaDireita = ">";

    private Coroutine mostrarCoroutine;
    private Coroutine animacaoBotoesCoroutine;
    private Transform alvoIris;
    private RawImage imagemIris;
    private Texture2D texturaIris;
    private Color32[] pixelsIris;
    private readonly List<BotaoMorteAnimado> botoesAnimados = new List<BotaoMorteAnimado>();
    private RectTransform setaEsquerda;
    private RectTransform setaDireita;
    private Text textoSetaEsquerdaUI;
    private Text textoSetaDireitaUI;
    private CanvasGroup grupoSetaEsquerda;
    private CanvasGroup grupoSetaDireita;
    private int indiceSelecionado = 0;
    private bool telaMorteVisivel = false;

    private void Awake()
    {
        Time.timeScale = 1f;
        EncontrarPainelMorte();
        ConfigurarBotoes();

        if (painelMorte != null)
            painelMorte.SetActive(false);
    }

    private void Update()
    {
        if (!telaMorteVisivel)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            MudarSelecao(1);

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            MudarSelecao(-1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            AtivarBotaoSelecionado();

        AtualizarSetasSelecao();
    }

    public void MostrarTelaMorte()
    {
        MostrarTelaMorte(null);
    }

    public void MostrarTelaMorte(Transform alvo)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (!enabled)
            enabled = true;

        EncontrarPainelMorte();
        EsconderPause();
        alvoIris = alvo;

        if (mostrarCoroutine != null)
            return;

        mostrarCoroutine = StartCoroutine(MostrarDepoisDoAtraso());
    }

    public void ReiniciarGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarParaMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nomeCenaMenu);
    }

    private IEnumerator MostrarDepoisDoAtraso()
    {
        yield return new WaitForSecondsRealtime(atrasoParaMostrar);

        if (usarTransicaoIris)
            yield return StartCoroutine(TocarTransicaoIris());

        if (painelMorte != null)
        {
            OrganizarPainelMorte();
            EstilizarBotoesComoTexto();
            CriarSetasSelecaoSePreciso();
            indiceSelecionado = Mathf.Clamp(indiceSelecionado, 0, botoesAnimados.Count - 1);
            AtualizarSetasSelecao();
            painelMorte.SetActive(true);
            telaMorteVisivel = true;
            TocarAnimacaoBotoes(true);
        }

        if (pausarJogoAoMostrar)
            Time.timeScale = 0f;
    }

    private IEnumerator TocarTransicaoIris()
    {
        CriarIrisSePreciso();

        imagemIris.gameObject.SetActive(true);

        float tempo = 0f;

        while (tempo < duracaoTransicaoIris)
        {
            float progresso = tempo / duracaoTransicaoIris;
            float suavizado = Mathf.SmoothStep(0f, 1f, progresso);
            float raio = Mathf.Lerp(raioInicialIris, raioFinalIris, suavizado);

            AtualizarMascaraIris(raio);

            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        AtualizarMascaraIris(0f);
    }

    private void CriarIrisSePreciso()
    {
        if (imagemIris != null)
            return;

        GameObject objetoIris = new GameObject("TransicaoIrisMorte");
        objetoIris.transform.SetParent(transform, false);

        imagemIris = objetoIris.AddComponent<RawImage>();
        imagemIris.raycastTarget = false;

        RectTransform rectTransform = imagemIris.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        texturaIris = new Texture2D(resolucaoMascaraIris, resolucaoMascaraIris, TextureFormat.RGBA32, false);
        texturaIris.wrapMode = TextureWrapMode.Clamp;
        texturaIris.filterMode = FilterMode.Bilinear;
        pixelsIris = new Color32[resolucaoMascaraIris * resolucaoMascaraIris];

        imagemIris.texture = texturaIris;
        imagemIris.gameObject.SetActive(false);
    }

    private void AtualizarMascaraIris(float raio)
    {
        Vector2 centro = new Vector2(0.5f, 0.5f);

        if (alvoIris != null && Camera.main != null)
        {
            Vector3 viewport = Camera.main.WorldToViewportPoint(alvoIris.position);
            centro = new Vector2(viewport.x, viewport.y);
        }

        float aspecto = Screen.width > 0 && Screen.height > 0 ? (float)Screen.width / Screen.height : 1f;
        int index = 0;

        for (int y = 0; y < resolucaoMascaraIris; y++)
        {
            float v = (float)y / (resolucaoMascaraIris - 1);

            for (int x = 0; x < resolucaoMascaraIris; x++)
            {
                float u = (float)x / (resolucaoMascaraIris - 1);
                float distanciaX = (u - centro.x) * aspecto;
                float distanciaY = v - centro.y;
                float distancia = Mathf.Sqrt(distanciaX * distanciaX + distanciaY * distanciaY);
                float mancha = CalcularManchaIris(u, v);
                float raioComMancha = raio + mancha * intensidadeManchaIris;
                float alpha = Mathf.InverseLerp(raioComMancha - suavidadeBordaIris, raioComMancha, distancia);
                alpha = Mathf.SmoothStep(0f, 1f, alpha);

                pixelsIris[index] = new Color32(0, 0, 0, (byte)(Mathf.Clamp01(alpha) * 255));
                index++;
            }
        }

        texturaIris.SetPixels32(pixelsIris);
        texturaIris.Apply(false);
    }

    private float CalcularManchaIris(float u, float v)
    {
        float ruidoGrande = Mathf.PerlinNoise(u * escalaManchaIris, v * escalaManchaIris);
        float ruidoPequeno = Mathf.PerlinNoise((u + 7.3f) * escalaManchaIris * 2.4f, (v + 3.8f) * escalaManchaIris * 2.4f);

        return ((ruidoGrande * 0.7f) + (ruidoPequeno * 0.3f) - 0.5f) * 2f;
    }

    private void ConfigurarBotoes()
    {
        EncontrarBotoes();

        if (botaoReiniciar != null)
        {
            botaoReiniciar.onClick.RemoveListener(ReiniciarGame);
            botaoReiniciar.onClick.AddListener(ReiniciarGame);
        }

        if (botaoMenu != null)
        {
            botaoMenu.onClick.RemoveListener(VoltarParaMenu);
            botaoMenu.onClick.AddListener(VoltarParaMenu);
        }
    }

    private void OrganizarPainelMorte()
    {
        Image fundoPainel = painelMorte.GetComponent<Image>();

        if (fundoPainel != null)
            fundoPainel.color = corFundoPainelMorte;

        painelMorte.transform.SetAsLastSibling();

        if (botaoReiniciar != null)
            botaoReiniciar.transform.SetAsLastSibling();

        if (botaoMenu != null)
            botaoMenu.transform.SetAsLastSibling();

        if (setaEsquerda != null)
            setaEsquerda.SetAsLastSibling();

        if (setaDireita != null)
            setaDireita.SetAsLastSibling();
    }

    private void EncontrarBotoes()
    {
        Button[] botoes = GetComponentsInChildren<Button>(true);

        foreach (Button botao in botoes)
        {
            string nome = botao.name.ToLower();

            if (botaoReiniciar == null && nome.Contains("reiniciar"))
                botaoReiniciar = botao;

            if (botaoMenu == null && (nome.Contains("menu") || nome.Contains("voltar")))
                botaoMenu = botao;
        }
    }

    private void EncontrarPainelMorte()
    {
        if (painelMorte != null && !painelMorte.name.ToLower().Contains("pause"))
            return;

        painelMorte = null;

        Transform painel = transform.Find(nomePainelMorte);

        if (painel != null)
        {
            painelMorte = painel.gameObject;
            return;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform item in transforms)
        {
            if (item.name == nomePainelMorte && item.gameObject.scene.IsValid())
            {
                painelMorte = item.gameObject;
                return;
            }
        }

        Debug.LogWarning("TelaMorteUI nao encontrou o PainelMorte. Crie/nomeie o painel como PainelMorte ou arraste ele no campo Painel Morte.");
    }

    private void EsconderPause()
    {
        PauseGame pauseGame = FindFirstObjectByType<PauseGame>(FindObjectsInactive.Include);

        if (pauseGame != null)
            pauseGame.DesativarPausePorMorte();

        if (botaoPause == null)
        {
            GameObject encontrado = GameObject.Find("ButtonPause");

            if (encontrado != null)
                botaoPause = encontrado;
        }

        if (botaoPause != null)
            botaoPause.SetActive(false);
    }

    private void EstilizarBotoesComoTexto()
    {
        botoesAnimados.Clear();

        EstilizarBotaoComoTexto(botaoReiniciar);
        EstilizarBotaoComoTexto(botaoMenu);

        if (EhLayoutMobile())
        {
            AplicarEspacamentoMobile();
            AtualizarPosicoesOriginaisBotoes();
        }
    }

    private void EstilizarBotaoComoTexto(Button botao)
    {
        if (botao == null)
            return;

        Image imagemBotao = botao.GetComponent<Image>();

        if (imagemBotao != null)
            imagemBotao.color = Color.clear;

        Text texto = botao.GetComponentInChildren<Text>(true);

        if (texto != null)
        {
            texto.color = corTextoBotoes;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.fontSize = Mathf.RoundToInt(ObterTamanhoTextoBotoes());
            texto.horizontalOverflow = HorizontalWrapMode.Overflow;
            texto.verticalOverflow = VerticalWrapMode.Overflow;
            AjustarRectTransformTexto(texto.GetComponent<RectTransform>());
        }

        TMP_Text textoTMP = botao.GetComponentInChildren<TMP_Text>(true);

        if (textoTMP != null)
        {
            textoTMP.color = corTextoBotoes;
            textoTMP.alignment = TextAlignmentOptions.Center;
            textoTMP.fontSize = ObterTamanhoTextoBotoes();
            textoTMP.textWrappingMode = TextWrappingModes.NoWrap;
            textoTMP.overflowMode = TextOverflowModes.Overflow;
            AjustarRectTransformTexto(textoTMP.GetComponent<RectTransform>());
        }

        RegistrarBotaoAnimado(botao);
    }

    private void RegistrarBotaoAnimado(Button botao)
    {
        if (botao == null)
            return;

        foreach (BotaoMorteAnimado existente in botoesAnimados)
        {
            if (existente.botao == botao)
                return;
        }

        botoesAnimados.Add(new BotaoMorteAnimado(botao));
    }

    private void CriarSetasSelecaoSePreciso()
    {
        if (EhLayoutMobile())
        {
            EsconderSetasSelecao();
            return;
        }

        if (setaEsquerda != null)
            setaEsquerda.gameObject.SetActive(true);

        if (setaDireita != null)
            setaDireita.gameObject.SetActive(true);

        if (painelMorte == null || setaEsquerda != null || setaDireita != null)
            return;

        setaEsquerda = CriarSetaSelecao("SetaMorteEsquerda", textoSetaEsquerda, out textoSetaEsquerdaUI);
        setaDireita = CriarSetaSelecao("SetaMorteDireita", textoSetaDireita, out textoSetaDireitaUI);
    }

    private RectTransform CriarSetaSelecao(string nome, string textoSeta, out Text textoUI)
    {
        GameObject objetoSeta = new GameObject(nome);
        objetoSeta.transform.SetParent(painelMorte.transform, false);

        CanvasGroup canvasGroup = objetoSeta.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        textoUI = objetoSeta.AddComponent<Text>();
        textoUI.text = textoSeta;
        textoUI.color = corTextoBotoes;
        textoUI.alignment = TextAnchor.MiddleCenter;
        textoUI.raycastTarget = false;
        textoUI.fontSize = ObterTamanhoTextoSetas();
        textoUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform rectTransform = objetoSeta.GetComponent<RectTransform>();
        float tamanhoSeta = Mathf.Max(42f, ObterTamanhoTextoSetas() + 10f);
        rectTransform.sizeDelta = new Vector2(tamanhoSeta, tamanhoSeta);

        if (nome.Contains("Esquerda"))
            grupoSetaEsquerda = canvasGroup;
        else
            grupoSetaDireita = canvasGroup;

        return rectTransform;
    }

    private void MudarSelecao(int direcao)
    {
        if (botoesAnimados.Count == 0)
            return;

        indiceSelecionado += direcao;

        if (indiceSelecionado < 0)
            indiceSelecionado = botoesAnimados.Count - 1;
        else if (indiceSelecionado >= botoesAnimados.Count)
            indiceSelecionado = 0;

        AtualizarSetasSelecao();
    }

    private void AtivarBotaoSelecionado()
    {
        if (indiceSelecionado < 0 || indiceSelecionado >= botoesAnimados.Count)
            return;

        botoesAnimados[indiceSelecionado].botao.onClick.Invoke();
    }

    private void AtualizarSetasSelecao()
    {
        if (EhLayoutMobile())
        {
            EsconderSetasSelecao();
            return;
        }

        if (setaEsquerda == null || setaDireita == null || botoesAnimados.Count == 0)
            return;

        BotaoMorteAnimado selecionado = botoesAnimados[Mathf.Clamp(indiceSelecionado, 0, botoesAnimados.Count - 1)];

        if (selecionado.rectTransform == null)
            return;

        Vector2 posicao = selecionado.rectTransform.anchoredPosition;
        setaEsquerda.anchoredPosition = posicao + Vector2.left * distanciaSetasSelecao;
        setaDireita.anchoredPosition = posicao + Vector2.right * distanciaSetasSelecao;
    }

    private void AjustarRectTransformTexto(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void AplicarEspacamentoMobile()
    {
        RectTransform reiniciarRect = botaoReiniciar != null ? botaoReiniciar.GetComponent<RectTransform>() : null;
        RectTransform menuRect = botaoMenu != null ? botaoMenu.GetComponent<RectTransform>() : null;

        if (reiniciarRect == null || menuRect == null)
            return;

        Vector2 centro = (reiniciarRect.anchoredPosition + menuRect.anchoredPosition) * 0.5f;
        reiniciarRect.anchoredPosition = new Vector2(reiniciarRect.anchoredPosition.x, centro.y + espacamentoBotoesMobile * 0.5f);
        menuRect.anchoredPosition = new Vector2(menuRect.anchoredPosition.x, centro.y - espacamentoBotoesMobile * 0.5f);
    }

    private void AtualizarPosicoesOriginaisBotoes()
    {
        for (int i = 0; i < botoesAnimados.Count; i++)
        {
            BotaoMorteAnimado botao = botoesAnimados[i];

            if (botao.rectTransform != null)
                botao.posicaoOriginal = botao.rectTransform.anchoredPosition;
        }
    }

    private void EsconderSetasSelecao()
    {
        if (setaEsquerda != null)
            setaEsquerda.gameObject.SetActive(false);

        if (setaDireita != null)
            setaDireita.gameObject.SetActive(false);
    }

    private void TocarAnimacaoBotoes(bool entrando)
    {
        if (animacaoBotoesCoroutine != null)
            StopCoroutine(animacaoBotoesCoroutine);

        animacaoBotoesCoroutine = StartCoroutine(AnimarBotoesMorte(entrando));
    }

    private IEnumerator AnimarBotoesMorte(bool entrando)
    {
        float tempo = 0f;
        float duracaoTotal = duracaoEntradaBotoes + atrasoEntreBotoes * Mathf.Max(0, botoesAnimados.Count - 1);

        while (tempo < duracaoTotal)
        {
            for (int i = 0; i < botoesAnimados.Count; i++)
            {
                BotaoMorteAnimado botao = botoesAnimados[i];
                float tempoBotao = tempo - atrasoEntreBotoes * i;
                float progresso = Mathf.Clamp01(tempoBotao / duracaoEntradaBotoes);

                if (!entrando)
                    progresso = 1f - progresso;

                AplicarAnimacaoBotao(botao, progresso);
            }

            float progressoSetas = Mathf.Clamp01(tempo / duracaoEntradaBotoes);

            if (!entrando)
                progressoSetas = 1f - progressoSetas;

            AplicarAnimacaoSetas(progressoSetas);

            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        for (int i = 0; i < botoesAnimados.Count; i++)
            AplicarAnimacaoBotao(botoesAnimados[i], entrando ? 1f : 0f);

        AplicarAnimacaoSetas(entrando ? 1f : 0f);
    }

    private void AplicarAnimacaoBotao(BotaoMorteAnimado botao, float progresso)
    {
        float suavizado = Mathf.SmoothStep(0f, 1f, progresso);
        float deslocamento = Mathf.Lerp(deslocamentoEntradaBotoes, 0f, suavizado);
        float brilho = 1f + Mathf.Sin(Time.unscaledTime * velocidadeBrilhoBotoes) * intensidadeBrilhoBotoes * suavizado;
        Color cor = corTextoBotoes * brilho;
        cor.a = suavizado;

        if (botao.rectTransform != null)
            botao.rectTransform.anchoredPosition = botao.posicaoOriginal + Vector2.down * deslocamento;

        if (botao.canvasGroup != null)
            botao.canvasGroup.alpha = suavizado;

        if (botao.texto != null)
            botao.texto.color = cor;

        if (botao.textoTMP != null)
            botao.textoTMP.color = cor;
    }

    private void AplicarAnimacaoSetas(float progresso)
    {
        if (setaEsquerda == null || setaDireita == null)
            return;

        AtualizarSetasSelecao();

        float suavizado = Mathf.SmoothStep(0f, 1f, progresso);
        Color cor = corTextoBotoes;
        cor.a = suavizado;

        if (grupoSetaEsquerda != null)
            grupoSetaEsquerda.alpha = suavizado;

        if (grupoSetaDireita != null)
            grupoSetaDireita.alpha = suavizado;

        if (textoSetaEsquerdaUI != null)
            textoSetaEsquerdaUI.color = cor;

        if (textoSetaDireitaUI != null)
            textoSetaDireitaUI.color = cor;
    }

    private bool EhLayoutMobile()
    {
        if (forcarLayoutMobileNoEditor)
            return true;

        if (Application.isMobilePlatform)
            return true;

        if (Application.isEditor && detectarMobilePorSafeAreaNoEditor && Screen.width > 0 && Screen.height > 0)
        {
            Rect safeArea = Screen.safeArea;
            bool safeAreaDiferente = safeArea.xMin > 1f
                || safeArea.yMin > 1f
                || safeArea.xMax < Screen.width - 1f
                || safeArea.yMax < Screen.height - 1f;

            if (safeAreaDiferente)
                return true;
        }

        return false;
    }

    private float ObterTamanhoTextoBotoes()
    {
        return EhLayoutMobile() ? tamanhoTextoBotoesMobile : tamanhoTextoBotoes;
    }

    private int ObterTamanhoTextoSetas()
    {
        return EhLayoutMobile() ? tamanhoTextoSetasMobile : tamanhoTextoSetas;
    }

    private class BotaoMorteAnimado
    {
        public Button botao;
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public Text texto;
        public TMP_Text textoTMP;
        public Vector2 posicaoOriginal;

        public BotaoMorteAnimado(Button botao)
        {
            this.botao = botao;
            rectTransform = botao.GetComponent<RectTransform>();
            canvasGroup = botao.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = botao.gameObject.AddComponent<CanvasGroup>();

            texto = botao.GetComponentInChildren<Text>(true);
            textoTMP = botao.GetComponentInChildren<TMP_Text>(true);

            if (rectTransform != null)
                posicaoOriginal = rectTransform.anchoredPosition;
        }
    }
}
