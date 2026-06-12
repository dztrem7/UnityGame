using UnityEngine;

public class VidaHudLayout : MonoBehaviour
{
    public Vector2 margem = new Vector2(48f, 42f);
    public Vector2 tamanhoCoracao = new Vector2(62f, 50f);
    public float espacamento = 68f;
    public bool respeitarSafeArea = true;
    public bool aplicarSomenteMobile = true;
    public bool forcarLayoutMobileNoEditor = false;
    public bool detectarMobilePorAspectoNoEditor = false;
    public float aspectoMinimoCelularWide = 1.85f;
    public float aspectoMaximoTablet = 1.55f;

    private RectTransform rectTransform;
    private int ultimaLargura;
    private int ultimaAltura;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        AplicarLayout();
    }

    private void LateUpdate()
    {
        if (ultimaLargura == Screen.width && ultimaAltura == Screen.height)
            return;

        AplicarLayout();
    }

    private void AplicarLayout()
    {
        ultimaLargura = Screen.width;
        ultimaAltura = Screen.height;

        if (aplicarSomenteMobile && !EhLayoutMobile())
            return;

        RectTransform[] coracoesCheios = ObterCoracoes("CoracaoCheio");
        RectTransform[] coracoesVazios = ObterCoracoes("CoracaoVazio");
        Vector2 margemFinal = margem + ObterMargemSafeArea();

        PosicionarCoracoes(coracoesVazios, margemFinal);
        PosicionarCoracoes(coracoesCheios, margemFinal);
    }

    private RectTransform[] ObterCoracoes(string prefixo)
    {
        RectTransform[] todos = GetComponentsInChildren<RectTransform>(true);
        RectTransform[] encontrados = new RectTransform[3];

        foreach (RectTransform item in todos)
        {
            if (!item.name.StartsWith(prefixo))
                continue;

            int indice = ObterIndice(item.name);

            if (indice >= 0 && indice < encontrados.Length)
                encontrados[indice] = item;
        }

        return encontrados;
    }

    private int ObterIndice(string nome)
    {
        if (nome.EndsWith("(1)"))
            return 1;

        if (nome.EndsWith("(2)"))
            return 2;

        return 0;
    }

    private void PosicionarCoracoes(RectTransform[] coracoes, Vector2 margemFinal)
    {
        for (int i = 0; i < coracoes.Length; i++)
        {
            RectTransform coracao = coracoes[i];

            if (coracao == null)
                continue;

            coracao.anchorMin = new Vector2(0f, 1f);
            coracao.anchorMax = new Vector2(0f, 1f);
            coracao.pivot = new Vector2(0f, 1f);
            coracao.sizeDelta = tamanhoCoracao;
            coracao.anchoredPosition = new Vector2(margemFinal.x + i * espacamento, -margemFinal.y);
        }
    }

    private Vector2 ObterMargemSafeArea()
    {
        if (!respeitarSafeArea || rectTransform == null || Screen.width <= 0 || Screen.height <= 0)
            return Vector2.zero;

        Rect safeArea = Screen.safeArea;
        Rect canvasRect = rectTransform.rect;
        float margemEsquerda = safeArea.xMin / Screen.width * canvasRect.width;
        float margemTopo = (Screen.height - safeArea.yMax) / Screen.height * canvasRect.height;

        return new Vector2(Mathf.Max(0f, margemEsquerda), Mathf.Max(0f, margemTopo));
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
}
