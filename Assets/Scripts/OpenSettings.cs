using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PauseGame : MonoBehaviour
{
    private bool pausado = false;

    public GameObject painelPause;
    public GameObject botaoPause;
    public Button botaoContinuar;
    public Button botaoReiniciar;
    public Button botaoMenu;
    public string nomeCenaMenu = "TelaInicial";
    public Color corTextoBotoes = Color.white;
    public bool estilizarBotaoPause = true;
    public Color corFundoBotaoPause = new Color(0f, 0f, 0f, 0.55f);
    public Color corIconeBotaoPause = new Color(0.9f, 0.96f, 1f, 1f);
    public Color corBordaBotaoPause = new Color(0.85f, 0.94f, 1f, 0.85f);
    public Vector2 tamanhoBotaoPause = new Vector2(58f, 48f);
    public Vector2 tamanhoBotaoPauseMobile = new Vector2(128f, 108f);
    public Vector2 margemBotaoPause = new Vector2(24f, 24f);
    public Vector2 margemBotaoPauseMobile = new Vector2(42f, 42f);
    public string iconeJogoRodando = "II";
    public string iconeJogoPausado = "\u25B6";
    public int tamanhoIconePauseRodando = 28;
    public int tamanhoIconePausePausado = 24;
    public int tamanhoIconePauseRodandoMobile = 58;
    public int tamanhoIconePausePausadoMobile = 52;
    public float tamanhoTextoBotoes = 18f;
    public float tamanhoTextoBotoesMobile = 58f;
    public int tamanhoTextoSetas = 34;
    public int tamanhoTextoSetasMobile = 66;
    public float alphaCentroVignette = 0.55f;
    public float alphaBordaVignette = 0.95f;
    public float forcaVignette = 1.8f;
    public float duracaoFadePause = 0.55f;
    public float duracaoEntradaBotoes = 0.65f;
    public float atrasoEntreBotoes = 0.12f;
    public float deslocamentoEntradaBotoes = 18f;
    public float velocidadeBrilhoBotoes = 2.2f;
    public float intensidadeBrilhoBotoes = 0.22f;
    public float distanciaSetasSelecao = 95f;
    public float espacamentoVerticalBotoes = 68f;
    public float espacamentoVerticalBotoesMobile = 118f;
    public bool fixarBotoesPauseNaCena = true;
    public bool forcarLayoutMobileNoEditor = false;
    public bool detectarMobilePorAspectoNoEditor = false;
    public float aspectoMinimoCelularWide = 1.85f;
    public float aspectoMaximoTablet = 1.55f;
    public string textoSetaEsquerda = "<";
    public string textoSetaDireita = ">";
    public int resolucaoVignette = 256;

    private bool bloqueadoPorMorte = false;
    private RawImage fundoVignette;
    private Texture2D texturaVignette;
    private Coroutine fadePauseCoroutine;
    private Coroutine animacaoBotoesCoroutine;
    private readonly List<BotaoPauseAnimado> botoesAnimados = new List<BotaoPauseAnimado>();
    private RectTransform setaEsquerda;
    private RectTransform setaDireita;
    private Text textoSetaEsquerdaUI;
    private Text textoSetaDireitaUI;
    private CanvasGroup grupoSetaEsquerda;
    private CanvasGroup grupoSetaDireita;
    private int indiceSelecionado = 0;
    private Text textoPauseUI;
    private TMP_Text textoPauseTMP;

    private void Awake()
    {
        Time.timeScale = 1f;
        EncontrarReferencias();
        ConfigurarBotoes();
        EstilizarBotaoPause();

        if (painelPause != null)
            painelPause.SetActive(false);

        if (botaoPause != null)
            botaoPause.SetActive(true);

        AtualizarIconeBotaoPause();
    }

    private void Update()
    {
        if (!pausado || bloqueadoPorMorte)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            MudarSelecao(1);

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            MudarSelecao(-1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            AtivarBotaoSelecionado();

        AtualizarSetasSelecao();
    }

    public void TogglePause()
    {
        if (bloqueadoPorMorte)
            return;

        if (pausado)
        {
            ContinuarGame();
        }
        else
        {
            PausarGame();
        }
    }

    public void PausarGame()
    {
        if (bloqueadoPorMorte)
            return;

        EncontrarReferencias();

        pausado = true;
        Time.timeScale = 0f;

        if (painelPause != null)
        {
            OrganizarPainelPause();
            CriarVignettePauseSePreciso();
            EstilizarBotoesComoTexto();
            CriarSetasSelecaoSePreciso();
            indiceSelecionado = Mathf.Clamp(indiceSelecionado, 0, botoesAnimados.Count - 1);
            AtualizarSetasSelecao();
            painelPause.SetActive(true);
            TocarFadePause(true);
            TocarAnimacaoBotoes(true);
        }
        else
            Debug.LogWarning("Pause ativado, mas nenhum PainelPause foi encontrado. Arraste o painel no campo Painel Pause ou nomeie ele como PainelPause.");

        AtualizarIconeBotaoPause();
    }

    public void ContinuarGame()
    {
        pausado = false;
        Time.timeScale = 1f;

        if (painelPause != null)
        {
            TocarAnimacaoBotoes(false);
            TocarFadePause(false);
        }

        AtualizarIconeBotaoPause();
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

    public void DesativarPausePorMorte()
    {
        bloqueadoPorMorte = true;
        pausado = false;

        if (painelPause != null)
            painelPause.SetActive(false);

        if (botaoPause != null)
            botaoPause.SetActive(false);
    }

    private void EncontrarReferencias()
    {
        if (painelPause == null)
        {
            GameObject encontrado = EncontrarObjetoPorNome("PainelPause");

            if (encontrado == null)
                encontrado = EncontrarObjetoPorNome("PanelPause");

            if (encontrado != null)
                painelPause = encontrado;
        }

        if (botaoPause == null)
        {
            GameObject encontrado = EncontrarObjetoPorNome("ButtonPause");

            if (encontrado != null)
                botaoPause = encontrado;
        }

        Button[] botoes = GetComponentsInChildren<Button>(true);

        foreach (Button botao in botoes)
        {
            string nome = botao.name.ToLower();

            if (botaoContinuar == null && (nome.Contains("continuar") || nome.Contains("resume")))
                botaoContinuar = botao;

            if (botaoReiniciar == null && nome.Contains("reiniciar"))
                botaoReiniciar = botao;

            if (botaoMenu == null && (nome.Contains("menu") || nome.Contains("voltar")))
                botaoMenu = botao;
        }
    }

    private void ConfigurarBotoes()
    {
        if (botaoContinuar != null)
        {
            botaoContinuar.onClick.RemoveListener(ContinuarGame);
            botaoContinuar.onClick.AddListener(ContinuarGame);
        }

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

    private void EstilizarBotaoPause()
    {
        if (!estilizarBotaoPause || botaoPause == null)
            return;

        Button botao = botaoPause.GetComponent<Button>();
        Image imagem = botaoPause.GetComponent<Image>();
        RectTransform rectTransform = botaoPause.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.one;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = Vector2.one;
            Vector2 margem = EhLayoutMobile() ? margemBotaoPauseMobile : margemBotaoPause;
            rectTransform.anchoredPosition = new Vector2(-margem.x, -margem.y);
            rectTransform.sizeDelta = EhLayoutMobile() ? tamanhoBotaoPauseMobile : tamanhoBotaoPause;
        }

        if (imagem != null)
        {
            imagem.color = corFundoBotaoPause;

            Outline borda = imagem.GetComponent<Outline>();

            if (borda == null)
                borda = imagem.gameObject.AddComponent<Outline>();

            borda.effectColor = corBordaBotaoPause;
            borda.effectDistance = new Vector2(1.4f, -1.4f);

            Shadow sombra = imagem.GetComponent<Shadow>();

            if (sombra == null)
                sombra = imagem.gameObject.AddComponent<Shadow>();

            sombra.effectColor = new Color(0f, 0f, 0f, 0.65f);
            sombra.effectDistance = new Vector2(2f, -2f);
        }

        if (botao != null)
        {
            ColorBlock cores = botao.colors;
            cores.normalColor = Color.white;
            cores.highlightedColor = new Color(0.8f, 0.92f, 1f, 1f);
            cores.pressedColor = new Color(0.55f, 0.75f, 0.9f, 1f);
            cores.selectedColor = new Color(0.8f, 0.92f, 1f, 1f);
            cores.colorMultiplier = 1f;
            botao.colors = cores;
        }

        LimparTextosDuplicadosBotaoPause();

        TMP_Text textoTMP = botaoPause.GetComponentInChildren<TMP_Text>(true);
        textoPauseTMP = textoTMP;

        if (textoPauseTMP != null)
        {
            textoPauseTMP.text = pausado ? iconeJogoPausado : iconeJogoRodando;
            textoPauseTMP.color = corIconeBotaoPause;
            textoPauseTMP.fontSize = ObterTamanhoIconePause();
            textoPauseTMP.fontStyle = FontStyles.Bold;
            textoPauseTMP.alignment = TextAlignmentOptions.Center;
            textoPauseTMP.raycastTarget = false;
        }
        else
        {
            Text textoExistente = botaoPause.GetComponentInChildren<Text>(true);

            if (textoExistente == null)
                textoExistente = CriarTextoBotaoPause();

            textoPauseUI = textoExistente;
            textoExistente.text = pausado ? iconeJogoPausado : iconeJogoRodando;
            textoExistente.color = corIconeBotaoPause;
            textoExistente.fontSize = Mathf.RoundToInt(ObterTamanhoIconePause());
            textoExistente.fontStyle = FontStyle.Bold;
            textoExistente.alignment = TextAnchor.MiddleCenter;
            textoExistente.raycastTarget = false;
        }

        RectTransform textoRect = textoPauseTMP != null
            ? textoPauseTMP.GetComponent<RectTransform>()
            : textoPauseUI.GetComponent<RectTransform>();

        if (textoRect != null)
        {
            textoRect.anchorMin = Vector2.zero;
            textoRect.anchorMax = Vector2.one;
            textoRect.offsetMin = Vector2.zero;
            textoRect.offsetMax = Vector2.zero;
        }

        GameObject objetoTexto = textoPauseTMP != null ? textoPauseTMP.gameObject : textoPauseUI.gameObject;
        Outline contornoIcone = objetoTexto.GetComponent<Outline>();

        if (contornoIcone == null)
            contornoIcone = objetoTexto.AddComponent<Outline>();

        contornoIcone.effectColor = new Color(0f, 0f, 0f, 0.9f);
        contornoIcone.effectDistance = new Vector2(1f, -1f);

        AtualizarIconeBotaoPause();
    }

    private void LimparTextosDuplicadosBotaoPause()
    {
        TMP_Text[] textosTMP = botaoPause.GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < textosTMP.Length; i++)
        {
            if (i == 0)
                continue;

            textosTMP[i].gameObject.SetActive(false);
        }

        Text[] textos = botaoPause.GetComponentsInChildren<Text>(true);

        for (int i = 0; i < textos.Length; i++)
        {
            if (i == 0)
                continue;

            textos[i].gameObject.SetActive(false);
        }
    }

    private Text CriarTextoBotaoPause()
    {
        GameObject textoObjeto = new GameObject("IconePause");
        textoObjeto.transform.SetParent(botaoPause.transform, false);

        Text texto = textoObjeto.AddComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return texto;
    }

    private void AtualizarIconeBotaoPause()
    {
        string icone = pausado ? iconeJogoPausado : iconeJogoRodando;

        if (textoPauseUI == null && botaoPause != null)
            textoPauseUI = botaoPause.GetComponentInChildren<Text>(true);

        if (textoPauseTMP == null && botaoPause != null)
            textoPauseTMP = botaoPause.GetComponentInChildren<TMP_Text>(true);

        if (textoPauseUI != null)
        {
            textoPauseUI.text = icone;
            textoPauseUI.color = corIconeBotaoPause;
        }

        if (textoPauseTMP != null)
        {
            textoPauseTMP.text = icone;
            textoPauseTMP.color = corIconeBotaoPause;
        }
    }

    private void OrganizarPainelPause()
    {
        Image fundoPainel = painelPause.GetComponent<Image>();

        if (fundoPainel != null)
            fundoPainel.color = Color.clear;

        painelPause.transform.SetAsLastSibling();

        if (fundoVignette != null)
            fundoVignette.transform.SetAsFirstSibling();

        if (botaoContinuar != null)
            botaoContinuar.transform.SetAsLastSibling();

        if (botaoReiniciar != null)
            botaoReiniciar.transform.SetAsLastSibling();

        if (botaoMenu != null)
            botaoMenu.transform.SetAsLastSibling();

        if (setaEsquerda != null)
            setaEsquerda.SetAsLastSibling();

        if (setaDireita != null)
            setaDireita.SetAsLastSibling();
    }

    private void CriarVignettePauseSePreciso()
    {
        if (painelPause == null || fundoVignette != null)
            return;

        GameObject objetoFundo = new GameObject("FundoVignettePause");
        objetoFundo.transform.SetParent(painelPause.transform, false);

        fundoVignette = objetoFundo.AddComponent<RawImage>();
        fundoVignette.raycastTarget = false;

        RectTransform rectTransform = fundoVignette.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        texturaVignette = new Texture2D(resolucaoVignette, resolucaoVignette, TextureFormat.RGBA32, false);
        texturaVignette.wrapMode = TextureWrapMode.Clamp;
        texturaVignette.filterMode = FilterMode.Bilinear;

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

        texturaVignette.SetPixels32(pixels);
        texturaVignette.Apply(false);

        fundoVignette.texture = texturaVignette;
        fundoVignette.color = new Color(1f, 1f, 1f, 0f);
        fundoVignette.transform.SetAsFirstSibling();
    }

    private void TocarFadePause(bool abrir)
    {
        if (fundoVignette == null)
        {
            if (!abrir && painelPause != null)
                painelPause.SetActive(false);

            return;
        }

        if (fadePauseCoroutine != null)
            StopCoroutine(fadePauseCoroutine);

        fadePauseCoroutine = StartCoroutine(AnimarFadePause(abrir));
    }

    private System.Collections.IEnumerator AnimarFadePause(bool abrir)
    {
        float inicio = fundoVignette.color.a;
        float fim = abrir ? 1f : 0f;
        float tempo = 0f;

        while (tempo < duracaoFadePause)
        {
            float progresso = tempo / duracaoFadePause;
            float suavizado = Mathf.SmoothStep(0f, 1f, progresso);
            float alpha = Mathf.Lerp(inicio, fim, suavizado);
            fundoVignette.color = new Color(1f, 1f, 1f, alpha);

            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        fundoVignette.color = new Color(1f, 1f, 1f, fim);

        if (!abrir && painelPause != null)
            painelPause.SetActive(false);
    }

    private void EstilizarBotoesComoTexto()
    {
        EstilizarBotaoComoTexto(botaoContinuar);
        EstilizarBotaoComoTexto(botaoReiniciar);
        EstilizarBotaoComoTexto(botaoMenu);
        AplicarEspacamentoBotoes();
    }

    private void AplicarEspacamentoBotoes()
    {
        int quantidade = botoesAnimados.Count;

        if (quantidade == 0)
            return;

        float espacamento = EhLayoutMobile() ? espacamentoVerticalBotoesMobile : espacamentoVerticalBotoes;
        float inicio = (quantidade - 1) * espacamento * 0.5f;
        float centroY = 0f;

        if (fixarBotoesPauseNaCena)
        {
            for (int i = 0; i < quantidade; i++)
                centroY += botoesAnimados[i].posicaoOriginal.y;

            centroY /= quantidade;
        }

        for (int i = 0; i < quantidade; i++)
        {
            BotaoPauseAnimado botao = botoesAnimados[i];

            if (botao.rectTransform == null)
                continue;

            Vector2 posicao = botao.posicaoOriginal;
            posicao.y = (fixarBotoesPauseNaCena ? centroY : 0f) + inicio - i * espacamento;
            botao.posicaoOriginal = posicao;
            botao.rectTransform.anchoredPosition = posicao;
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
        }

        TMP_Text textoTMP = botao.GetComponentInChildren<TMP_Text>(true);

        if (textoTMP != null)
        {
            textoTMP.color = corTextoBotoes;
            textoTMP.alignment = TextAlignmentOptions.Center;
            textoTMP.fontSize = ObterTamanhoTextoBotoes();
            textoTMP.enableAutoSizing = false;
            textoTMP.textWrappingMode = TextWrappingModes.NoWrap;
            textoTMP.overflowMode = TextOverflowModes.Overflow;
        }

        RegistrarBotaoAnimado(botao);

        RectTransform rectTransform = botao.GetComponent<RectTransform>();

        if (rectTransform != null && !fixarBotoesPauseNaCena)
            rectTransform.sizeDelta = EhLayoutMobile() ? new Vector2(420f, 92f) : new Vector2(160f, 30f);
    }

    private void RegistrarBotaoAnimado(Button botao)
    {
        if (botao == null)
            return;

        foreach (BotaoPauseAnimado existente in botoesAnimados)
        {
            if (existente.botao == botao)
                return;
        }

        botoesAnimados.Add(new BotaoPauseAnimado(botao));
    }

    private void CriarSetasSelecaoSePreciso()
    {
        if (painelPause == null || setaEsquerda != null || setaDireita != null)
            return;

        setaEsquerda = CriarSetaSelecao("SetaSelecaoEsquerda", textoSetaEsquerda, out textoSetaEsquerdaUI);
        setaDireita = CriarSetaSelecao("SetaSelecaoDireita", textoSetaDireita, out textoSetaDireitaUI);
    }

    private RectTransform CriarSetaSelecao(string nome, string textoSeta, out Text textoUI)
    {
        GameObject objetoSeta = new GameObject(nome);
        objetoSeta.transform.SetParent(painelPause.transform, false);

        CanvasGroup canvasGroup = objetoSeta.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        textoUI = objetoSeta.AddComponent<Text>();
        textoUI.text = textoSeta;
        textoUI.color = corTextoBotoes;
        textoUI.alignment = TextAnchor.MiddleCenter;
        textoUI.raycastTarget = false;
        textoUI.fontSize = EhLayoutMobile() ? tamanhoTextoSetasMobile : tamanhoTextoSetas;
        textoUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform rectTransform = objetoSeta.GetComponent<RectTransform>();
        rectTransform.sizeDelta = EhLayoutMobile() ? new Vector2(72f, 72f) : new Vector2(42f, 42f);

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
        if (setaEsquerda == null || setaDireita == null || botoesAnimados.Count == 0)
            return;

        BotaoPauseAnimado selecionado = botoesAnimados[Mathf.Clamp(indiceSelecionado, 0, botoesAnimados.Count - 1)];

        if (selecionado.rectTransform == null)
            return;

        Vector2 posicao = selecionado.rectTransform.anchoredPosition;

        setaEsquerda.anchoredPosition = posicao + Vector2.left * distanciaSetasSelecao;
        setaDireita.anchoredPosition = posicao + Vector2.right * distanciaSetasSelecao;

        if (textoSetaEsquerdaUI != null)
            textoSetaEsquerdaUI.color = corTextoBotoes;

        if (textoSetaDireitaUI != null)
            textoSetaDireitaUI.color = corTextoBotoes;
    }

    private void TocarAnimacaoBotoes(bool entrando)
    {
        if (animacaoBotoesCoroutine != null)
            StopCoroutine(animacaoBotoesCoroutine);

        animacaoBotoesCoroutine = StartCoroutine(AnimarBotoesPause(entrando));
    }

    private System.Collections.IEnumerator AnimarBotoesPause(bool entrando)
    {
        float tempo = 0f;
        float duracaoTotal = duracaoEntradaBotoes + atrasoEntreBotoes * Mathf.Max(0, botoesAnimados.Count - 1);

        while (tempo < duracaoTotal)
        {
            for (int i = 0; i < botoesAnimados.Count; i++)
            {
                BotaoPauseAnimado botao = botoesAnimados[i];
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

    private void AplicarAnimacaoBotao(BotaoPauseAnimado botao, float progresso)
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

    private float ObterTamanhoIconePause()
    {
        if (EhLayoutMobile())
            return pausado ? tamanhoIconePausePausadoMobile : tamanhoIconePauseRodandoMobile;

        return pausado ? tamanhoIconePausePausado : tamanhoIconePauseRodando;
    }

    private float ObterTamanhoTextoBotoes()
    {
        return EhLayoutMobile() ? tamanhoTextoBotoesMobile : tamanhoTextoBotoes;
    }

    private class BotaoPauseAnimado
    {
        public Button botao;
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public Text texto;
        public TMP_Text textoTMP;
        public Vector2 posicaoOriginal;

        public BotaoPauseAnimado(Button botao)
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

    private GameObject EncontrarObjetoPorNome(string nome)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform item in transforms)
        {
            if (item.name == nome && item.gameObject.scene.IsValid())
                return item.gameObject;
        }

        return null;
    }
}
