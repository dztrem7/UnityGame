using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class MobileControlsUI : MonoBehaviour
{
    public bool mostrarControlesParaEditar = true;
    public bool mostrarBotaoDesgrudarParaEditar = true;
    public bool forcarControlesMobileNoEditor = false;
    public bool detectarMobilePorAspectoNoEditor = false;
    public float aspectoMinimoCelularWide = 1.85f;
    public float aspectoMaximoTablet = 1.55f;
    public bool respeitarSafeArea = true;
    public bool usarPosicaoManualNaCena = true;
    public bool fixarBotoesNaCena = true;

    public Vector2 margemJoystick = new Vector2(62f, 54f);
    public Vector2 margemPulo = new Vector2(92f, 56f);
    public Vector2 margemAtaque = new Vector2(96f, 144f);
    public Vector2 margemInteracao = new Vector2(238f, 118f);
    public Vector2 margemEscaparGrude = new Vector2(228f, 218f);
    public float tamanhoJoystick = 156f;
    public float tamanhoBotaoPulo = 108f;
    public float tamanhoBotaoAtaque = 94f;
    public float tamanhoBotaoInteracao = 88f;
    public float tamanhoBotaoEscaparGrude = 90f;
    public float tamanhoTextoBotoes = 42f;

    public Color corFundo = new Color(0f, 0f, 0f, 0.46f);
    public Color corBorda = new Color(0.82f, 0.94f, 1f, 0.78f);
    public Color corIcone = new Color(0.92f, 0.98f, 1f, 0.92f);
    public Color corPressionado = new Color(0.72f, 0.9f, 1f, 0.78f);

    private Canvas canvas;
    private GameObject raizControles;
    private MobileJoystickControl joystick;
    private Vector2 ultimaMargemJoystick;
    private Vector2 ultimaMargemPulo;
    private Vector2 ultimaMargemAtaque;
    private Vector2 ultimaMargemInteracao;
    private Vector2 ultimaMargemEscaparGrude;
    private float ultimoTamanhoJoystick;
    private float ultimoTamanhoBotaoPulo;
    private float ultimoTamanhoBotaoAtaque;
    private float ultimoTamanhoBotaoInteracao;
    private float ultimoTamanhoBotaoEscaparGrude;
    private float ultimoTamanhoTextoBotoes;
    private Sprite spriteInteracao;

    private void Awake()
    {
        CriarUI();
        AtualizarVisibilidade();
    }

    private void OnEnable()
    {
        CriarUI();
        AtualizarLayout();
        AtualizarVisibilidade();
    }

    private void OnValidate()
    {
        CriarUI();
        AtualizarLayout();
        AtualizarVisibilidade();
    }

    private void Update()
    {
        if (LayoutMudou())
            AtualizarLayout();

        AtualizarVisibilidade();
    }

    private void CriarUI()
    {
        if (raizControles != null)
            return;

        Transform canvasExistente = transform.Find("CanvasControlesMobile");
        GameObject canvasObj = canvasExistente != null ? canvasExistente.gameObject : new GameObject("CanvasControlesMobile");
        canvasObj.transform.SetParent(transform, false);

        canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null)
            canvas = canvasObj.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvasObj.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObj.GetComponent<GraphicRaycaster>() == null)
            canvasObj.AddComponent<GraphicRaycaster>();

        Transform raizExistente = canvasObj.transform.Find("ControlesMobile");
        raizControles = raizExistente != null ? raizExistente.gameObject : new GameObject("ControlesMobile");
        raizControles.transform.SetParent(canvasObj.transform, false);
        RectTransform raizRect = raizControles.GetComponent<RectTransform>();
        if (raizRect == null)
            raizRect = raizControles.AddComponent<RectTransform>();

        raizRect.anchorMin = Vector2.zero;
        raizRect.anchorMax = Vector2.one;
        raizRect.offsetMin = Vector2.zero;
        raizRect.offsetMax = Vector2.zero;

        if (raizRect.Find("JoystickMoverMobile") == null)
            CriarJoystick(raizRect);

        if (raizRect.Find("BotaoPularMobile") == null)
            CriarBotao(raizRect, "BotaoPularMobile", "^", tamanhoBotaoPulo, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margemPulo.x, margemPulo.y), MobileControlsInput.PressionarPulo);

        if (raizRect.Find("BotaoAtaqueMobile") == null)
            CriarBotao(raizRect, "BotaoAtaqueMobile", "<", tamanhoBotaoAtaque, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-margemAtaque.x, -margemAtaque.y), MobileControlsInput.PressionarAtaque);

        if (raizRect.Find("BotaoInteragirMobile") == null)
            CriarBotao(raizRect, "BotaoInteragirMobile", "", tamanhoBotaoInteracao, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margemInteracao.x, margemInteracao.y), MobileControlsInput.PressionarInteracao);

        if (raizRect.Find("BotaoEscaparGrudeMobile") == null)
            CriarBotao(raizRect, "BotaoEscaparGrudeMobile", "!", tamanhoBotaoEscaparGrude, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margemEscaparGrude.x, margemEscaparGrude.y), MobileControlsInput.PressionarEscaparGrude);

        ConfigurarBotaoAcao(raizRect, "BotaoPularMobile", MobileActionButton.Acao.Pular);
        ConfigurarBotaoAcao(raizRect, "BotaoAtaqueMobile", MobileActionButton.Acao.Atacar);
        ConfigurarBotaoAcao(raizRect, "BotaoInteragirMobile", MobileActionButton.Acao.Interagir);
        ConfigurarBotaoAcao(raizRect, "BotaoEscaparGrudeMobile", MobileActionButton.Acao.EscaparGrude);
        AtualizarVisibilidadeBotoesContextuais();
        AtualizarLayout();
    }

    private void CriarJoystick(RectTransform pai)
    {
        GameObject baseObj = CriarCirculo("JoystickMoverMobile", pai, tamanhoJoystick, corFundo, corBorda);
        RectTransform rect = baseObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = margemJoystick + ObterMargemSafeAreaEsquerdaBaixo() + Vector2.one * (tamanhoJoystick * 0.5f);

        GameObject miolo = CriarCirculo("MioloJoystickMobile", rect, tamanhoJoystick * 0.42f, new Color(1f, 1f, 1f, 0.08f), corIcone);
        RectTransform mioloRect = miolo.GetComponent<RectTransform>();
        mioloRect.anchorMin = new Vector2(0.5f, 0.5f);
        mioloRect.anchorMax = new Vector2(0.5f, 0.5f);
        mioloRect.pivot = new Vector2(0.5f, 0.5f);
        mioloRect.anchoredPosition = Vector2.zero;

        joystick = baseObj.GetComponent<MobileJoystickControl>();
        if (joystick == null)
            joystick = baseObj.AddComponent<MobileJoystickControl>();

        joystick.miolo = mioloRect;
        joystick.raio = tamanhoJoystick * 0.34f;
        joystick.corNormal = corFundo;
        joystick.corPressionado = corPressionado;
        joystick.imagemBase = baseObj.GetComponent<Image>();
    }

    private void CriarBotao(RectTransform pai, string nome, string texto, float tamanho, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 posicao, System.Action acao)
    {
        GameObject botaoObj = CriarCirculo(nome, pai, tamanho, corFundo, corBorda);
        RectTransform rect = botaoObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = posicao + ObterMargemSafeAreaDireita(anchorMin, anchorMax);

        Button botao = botaoObj.GetComponent<Button>();
        if (botao == null)
            botao = botaoObj.AddComponent<Button>();

        botao.transition = Selectable.Transition.ColorTint;
        botao.colors = CriarCoresBotao();
        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(() => acao());

        Transform textoExistente = botaoObj.transform.Find("Icone");
        GameObject textoObj = textoExistente != null ? textoExistente.gameObject : new GameObject("Icone");
        textoObj.transform.SetParent(botaoObj.transform, false);
        Text textoUI = textoObj.GetComponent<Text>();
        if (textoUI == null)
            textoUI = textoObj.AddComponent<Text>();

        textoUI.text = texto;
        textoUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoUI.fontSize = Mathf.RoundToInt(tamanhoTextoBotoes);
        textoUI.fontStyle = FontStyle.Bold;
        textoUI.alignment = TextAnchor.MiddleCenter;
        textoUI.color = corIcone;
        textoUI.raycastTarget = false;

        RectTransform textoRect = textoObj.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;
    }

    private void ConfigurarBotaoAcao(RectTransform pai, string nome, MobileActionButton.Acao acao)
    {
        Transform encontrado = pai.Find(nome);
        if (encontrado == null)
            return;

        Button botao = encontrado.GetComponent<Button>();
        if (botao == null)
            botao = encontrado.gameObject.AddComponent<Button>();

        botao.transition = Selectable.Transition.ColorTint;
        botao.colors = CriarCoresBotao();

        MobileActionButton controle = encontrado.GetComponent<MobileActionButton>();
        if (controle == null)
            controle = encontrado.gameObject.AddComponent<MobileActionButton>();

        controle.acao = acao;
        controle.imagem = encontrado.GetComponent<Image>();
        controle.corNormal = Color.white;
        controle.corPressionado = corPressionado;

        RestaurarIconePadrao(encontrado, acao);

        if (acao == MobileActionButton.Acao.Interagir)
            EstilizarBotaoInteracao(encontrado);
    }

    private void RestaurarIconePadrao(Transform botao, MobileActionButton.Acao acao)
    {
        if (acao == MobileActionButton.Acao.Interagir)
            return;

        Transform iconePulo = botao.Find("IconePulo");
        if (iconePulo != null)
            iconePulo.gameObject.SetActive(false);

        Transform iconeAtaque = botao.Find("IconeAtaque");
        if (iconeAtaque != null)
            iconeAtaque.gameObject.SetActive(false);

        Transform iconeTexto = botao.Find("Icone");
        if (iconeTexto != null)
            iconeTexto.gameObject.SetActive(true);

        Text texto = botao.GetComponentInChildren<Text>(true);
        if (texto == null)
            return;

        texto.text = acao == MobileActionButton.Acao.Pular ? "^" : acao == MobileActionButton.Acao.EscaparGrude ? "!" : "<";
        texto.color = corIcone;
        texto.fontSize = Mathf.RoundToInt(tamanhoTextoBotoes);
        texto.fontStyle = FontStyle.Bold;
        texto.alignment = TextAnchor.MiddleCenter;
    }

    private void EstilizarBotaoInteracao(Transform botao)
    {
        Text texto = botao.GetComponentInChildren<Text>(true);

        if (texto != null)
            texto.text = "";

        Transform iconeTexto = botao.Find("Icone");

        if (iconeTexto != null)
            iconeTexto.gameObject.SetActive(false);

        Transform iconeExistente = botao.Find("IconeInteracao");
        GameObject iconeObj = iconeExistente != null ? iconeExistente.gameObject : new GameObject("IconeInteracao");
        iconeObj.transform.SetParent(botao, false);

        Image imagemIcone = iconeObj.GetComponent<Image>();
        if (imagemIcone == null)
            imagemIcone = iconeObj.AddComponent<Image>();

        if (spriteInteracao == null)
            spriteInteracao = CriarSpriteInteracao(96);

        imagemIcone.sprite = spriteInteracao;
        imagemIcone.color = corIcone;
        imagemIcone.raycastTarget = false;

        RectTransform rect = iconeObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(18f, 16f);
        rect.offsetMax = new Vector2(-18f, -14f);
    }


    private GameObject CriarCirculo(string nome, Transform pai, float tamanho, Color fundo, Color borda)
    {
        GameObject obj = new GameObject(nome);
        obj.transform.SetParent(pai, false);

        Image imagem = obj.AddComponent<Image>();
        imagem.sprite = CriarSpriteCircular(96, fundo, borda);
        imagem.type = Image.Type.Sliced;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(tamanho, tamanho);

        return obj;
    }

    private Sprite CriarSpriteCircular(int tamanho, Color fundo, Color borda)
    {
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Bilinear;

        Vector2 centro = new Vector2((tamanho - 1) * 0.5f, (tamanho - 1) * 0.5f);
        float raio = tamanho * 0.48f;
        float bordaLargura = tamanho * 0.08f;

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), centro);
                Color cor = Color.clear;

                if (distancia <= raio)
                    cor = distancia >= raio - bordaLargura ? borda : fundo;

                textura.SetPixel(x, y, cor);
            }
        }

        textura.Apply();
        return Sprite.Create(textura, new Rect(0f, 0f, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(16f, 16f, 16f, 16f));
    }

    private Sprite CriarSpriteInteracao(int tamanho)
    {
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Bilinear;

        Color claro = Color.white;
        Color sombra = new Color(0f, 0f, 0f, 0.55f);

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
                textura.SetPixel(x, y, Color.clear);
        }

        DesenharIconeToque(textura, sombra, 8f, new Vector2(2f, -2f));
        DesenharIconeToque(textura, claro, 5.5f, Vector2.zero);

        textura.Apply();
        return Sprite.Create(textura, new Rect(0f, 0f, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100f);
    }

    private void DesenharIconeToque(Texture2D textura, Color cor, float espessura, Vector2 deslocamento)
    {
        DesenharLinha(textura, new Vector2(38f, 28f) + deslocamento, new Vector2(34f, 70f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(34f, 70f) + deslocamento, new Vector2(41f, 78f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(41f, 78f) + deslocamento, new Vector2(49f, 71f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(49f, 71f) + deslocamento, new Vector2(51f, 52f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(51f, 52f) + deslocamento, new Vector2(63f, 51f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(63f, 51f) + deslocamento, new Vector2(74f, 43f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(74f, 43f) + deslocamento, new Vector2(75f, 25f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(75f, 25f) + deslocamento, new Vector2(63f, 14f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(63f, 14f) + deslocamento, new Vector2(37f, 14f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(37f, 14f) + deslocamento, new Vector2(25f, 25f) + deslocamento, espessura, cor);
        DesenharLinha(textura, new Vector2(25f, 25f) + deslocamento, new Vector2(38f, 28f) + deslocamento, espessura, cor);

        DesenharLinha(textura, new Vector2(29f, 83f) + deslocamento, new Vector2(23f, 91f) + deslocamento, espessura * 0.7f, cor);
        DesenharLinha(textura, new Vector2(43f, 86f) + deslocamento, new Vector2(43f, 96f) + deslocamento, espessura * 0.7f, cor);
        DesenharLinha(textura, new Vector2(57f, 82f) + deslocamento, new Vector2(65f, 90f) + deslocamento, espessura * 0.7f, cor);
    }

    private void DesenharLinha(Texture2D textura, Vector2 inicio, Vector2 fim, float espessura, Color cor)
    {
        int passos = Mathf.CeilToInt(Vector2.Distance(inicio, fim) * 2f);

        for (int i = 0; i <= passos; i++)
        {
            Vector2 ponto = Vector2.Lerp(inicio, fim, passos == 0 ? 0f : (float)i / passos);
            PintarCirculo(textura, ponto, espessura * 0.5f, cor);
        }
    }

    private void PintarCirculo(Texture2D textura, Vector2 centro, float raio, Color cor)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(centro.x - raio));
        int maxX = Mathf.Min(textura.width - 1, Mathf.CeilToInt(centro.x + raio));
        int minY = Mathf.Max(0, Mathf.FloorToInt(centro.y - raio));
        int maxY = Mathf.Min(textura.height - 1, Mathf.CeilToInt(centro.y + raio));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (Vector2.Distance(new Vector2(x, y), centro) > raio)
                    continue;

                Color atual = textura.GetPixel(x, y);
                textura.SetPixel(x, y, Color.Lerp(atual, cor, cor.a));
            }
        }
    }

    private ColorBlock CriarCoresBotao()
    {
        ColorBlock cores = ColorBlock.defaultColorBlock;
        cores.normalColor = Color.white;
        cores.highlightedColor = new Color(1f, 1f, 1f, 1.12f);
        cores.pressedColor = corPressionado;
        cores.selectedColor = Color.white;
        cores.disabledColor = new Color(1f, 1f, 1f, 0.28f);
        cores.colorMultiplier = 1f;
        return cores;
    }

    private void AtualizarVisibilidade()
    {
        bool editando = !Application.isPlaying && mostrarControlesParaEditar;
        bool ativo = editando || EhLayoutMobile();

        if (raizControles != null && raizControles.activeSelf != ativo)
            raizControles.SetActive(ativo);

        AtualizarVisibilidadeBotoesContextuais();

        if (!ativo)
            MobileControlsInput.Limpar();
    }

    private void AtualizarVisibilidadeBotoesContextuais()
    {
        if (raizControles == null)
            return;

        Transform botaoInteragir = raizControles.transform.Find("BotaoInteragirMobile");

        if (botaoInteragir != null && botaoInteragir.gameObject.activeSelf != MobileControlsInput.InteracaoDisponivel)
            botaoInteragir.gameObject.SetActive(MobileControlsInput.InteracaoDisponivel);

        Transform botaoEscapar = raizControles.transform.Find("BotaoEscaparGrudeMobile");
        bool mostrarEscapar = !Application.isPlaying && mostrarBotaoDesgrudarParaEditar || MobileControlsInput.EscaparGrudeDisponivel;

        if (botaoEscapar != null && botaoEscapar.gameObject.activeSelf != mostrarEscapar)
            botaoEscapar.gameObject.SetActive(mostrarEscapar);
    }

    private bool LayoutMudou()
    {
        if (fixarBotoesNaCena)
            return false;

        return ultimaMargemJoystick != margemJoystick
            || ultimaMargemPulo != margemPulo
            || ultimaMargemAtaque != margemAtaque
            || ultimaMargemInteracao != margemInteracao
            || ultimaMargemEscaparGrude != margemEscaparGrude
            || !Mathf.Approximately(ultimoTamanhoJoystick, tamanhoJoystick)
            || !Mathf.Approximately(ultimoTamanhoBotaoPulo, tamanhoBotaoPulo)
            || !Mathf.Approximately(ultimoTamanhoBotaoAtaque, tamanhoBotaoAtaque)
            || !Mathf.Approximately(ultimoTamanhoBotaoInteracao, tamanhoBotaoInteracao)
            || !Mathf.Approximately(ultimoTamanhoBotaoEscaparGrude, tamanhoBotaoEscaparGrude)
            || !Mathf.Approximately(ultimoTamanhoTextoBotoes, tamanhoTextoBotoes);
    }

    private void AtualizarLayout()
    {
        if (fixarBotoesNaCena)
        {
            ConfigurarComponentesExistentes();
            return;
        }

        if (raizControles == null)
            return;

        RectTransform raizRect = raizControles.GetComponent<RectTransform>();
        if (raizRect == null)
            return;

        AjustarJoystick(raizRect);
        AjustarBotao(raizRect, "BotaoPularMobile", tamanhoBotaoPulo, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margemPulo.x, margemPulo.y));
        AjustarBotao(raizRect, "BotaoAtaqueMobile", tamanhoBotaoAtaque, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-margemAtaque.x, -margemAtaque.y));
        AjustarBotao(raizRect, "BotaoInteragirMobile", tamanhoBotaoInteracao, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margemInteracao.x, margemInteracao.y));
        AjustarBotao(raizRect, "BotaoEscaparGrudeMobile", tamanhoBotaoEscaparGrude, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margemEscaparGrude.x, margemEscaparGrude.y));

        ultimaMargemJoystick = margemJoystick;
        ultimaMargemPulo = margemPulo;
        ultimaMargemAtaque = margemAtaque;
        ultimaMargemInteracao = margemInteracao;
        ultimaMargemEscaparGrude = margemEscaparGrude;
        ultimoTamanhoJoystick = tamanhoJoystick;
        ultimoTamanhoBotaoPulo = tamanhoBotaoPulo;
        ultimoTamanhoBotaoAtaque = tamanhoBotaoAtaque;
        ultimoTamanhoBotaoInteracao = tamanhoBotaoInteracao;
        ultimoTamanhoBotaoEscaparGrude = tamanhoBotaoEscaparGrude;
        ultimoTamanhoTextoBotoes = tamanhoTextoBotoes;
    }

    private void AjustarJoystick(RectTransform raizRect)
    {
        RectTransform rect = raizRect.Find("JoystickMoverMobile") as RectTransform;
        if (rect == null)
            return;

        if (!usarPosicaoManualNaCena)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(tamanhoJoystick, tamanhoJoystick);
            rect.anchoredPosition = margemJoystick + ObterMargemSafeAreaEsquerdaBaixo() + Vector2.one * (tamanhoJoystick * 0.5f);
        }

        float tamanhoAtual = Mathf.Min(Mathf.Abs(rect.sizeDelta.x), Mathf.Abs(rect.sizeDelta.y));
        if (tamanhoAtual <= 0f)
            tamanhoAtual = tamanhoJoystick;

        RectTransform miolo = rect.Find("MioloJoystickMobile") as RectTransform;
        if (miolo != null)
            miolo.sizeDelta = Vector2.one * (tamanhoAtual * 0.42f);

        MobileJoystickControl joystickAtual = rect.GetComponent<MobileJoystickControl>();
        if (joystickAtual != null)
            joystickAtual.raio = tamanhoAtual * 0.34f;
    }

    private void AjustarBotao(RectTransform raizRect, string nome, float tamanho, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 posicao)
    {
        RectTransform rect = raizRect.Find(nome) as RectTransform;
        if (rect == null)
            return;

        if (!usarPosicaoManualNaCena)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = new Vector2(tamanho, tamanho);
            rect.anchoredPosition = posicao + ObterMargemSafeAreaDireita(anchorMin, anchorMax);
        }

        Text texto = rect.GetComponentInChildren<Text>(true);
        if (texto != null && !usarPosicaoManualNaCena)
            texto.fontSize = Mathf.RoundToInt(tamanhoTextoBotoes);
    }

    private void ConfigurarComponentesExistentes()
    {
        if (raizControles == null)
            return;

        RectTransform raizRect = raizControles.GetComponent<RectTransform>();
        if (raizRect == null)
            return;

        RectTransform joystickRect = raizRect.Find("JoystickMoverMobile") as RectTransform;
        if (joystickRect != null)
        {
            MobileJoystickControl joystickAtual = joystickRect.GetComponent<MobileJoystickControl>();
            if (joystickAtual == null)
                joystickAtual = joystickRect.gameObject.AddComponent<MobileJoystickControl>();

            joystickAtual.miolo = joystickRect.Find("MioloJoystickMobile") as RectTransform;
            joystickAtual.imagemBase = joystickRect.GetComponent<Image>();
            joystickAtual.corNormal = corFundo;
            joystickAtual.corPressionado = corPressionado;

            float tamanhoAtual = Mathf.Min(Mathf.Abs(joystickRect.sizeDelta.x), Mathf.Abs(joystickRect.sizeDelta.y));
            if (tamanhoAtual > 0f)
                joystickAtual.raio = tamanhoAtual * 0.34f;
        }

        ConfigurarBotaoAcao(raizRect, "BotaoPularMobile", MobileActionButton.Acao.Pular);
        ConfigurarBotaoAcao(raizRect, "BotaoAtaqueMobile", MobileActionButton.Acao.Atacar);
        ConfigurarBotaoAcao(raizRect, "BotaoInteragirMobile", MobileActionButton.Acao.Interagir);
        ConfigurarBotaoAcao(raizRect, "BotaoEscaparGrudeMobile", MobileActionButton.Acao.EscaparGrude);
        AtualizarVisibilidadeBotoesContextuais();
    }

    private Vector2 ObterMargemSafeAreaEsquerdaBaixo()
    {
        if (!respeitarSafeArea || Screen.width <= 0 || Screen.height <= 0 || canvas == null)
            return Vector2.zero;

        Rect safeArea = Screen.safeArea;
        Rect canvasRect = ((RectTransform)canvas.transform).rect;
        return new Vector2(safeArea.xMin / Screen.width * canvasRect.width, safeArea.yMin / Screen.height * canvasRect.height);
    }

    private Vector2 ObterMargemSafeAreaDireita(Vector2 anchorMin, Vector2 anchorMax)
    {
        if (!respeitarSafeArea || Screen.width <= 0 || Screen.height <= 0 || canvas == null)
            return Vector2.zero;

        Rect safeArea = Screen.safeArea;
        Rect canvasRect = ((RectTransform)canvas.transform).rect;
        float margemDireita = (Screen.width - safeArea.xMax) / Screen.width * canvasRect.width;
        float margemTopo = (Screen.height - safeArea.yMax) / Screen.height * canvasRect.height;

        if (anchorMin.y >= 1f && anchorMax.y >= 1f)
            return new Vector2(-margemDireita, -margemTopo);

        return new Vector2(-margemDireita, safeArea.yMin / Screen.height * canvasRect.height);
    }

    private bool EhLayoutMobile()
    {
        if (forcarControlesMobileNoEditor)
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
        bool aspectoMobile = Application.isEditor
            && detectarMobilePorAspectoNoEditor
            && (aspecto >= aspectoMinimoCelularWide || aspecto <= aspectoMaximoTablet);

        return temSafeAreaDiferente || aspectoMobile;
    }

}
