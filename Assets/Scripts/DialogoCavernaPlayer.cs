using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogoCavernaPlayer : MonoBehaviour
{
    public string nomeColliderDialogo = "AtivarDialogoCaverna";
    [TextArea(2, 4)]
    public string fala = "Nossa, esta muito escuro aqui dentro, melhor eu tomar cuidado.";
    public float tempoNaTela = 4f;
    public bool falarUmaVez = true;

    [Header("Legenda")]
    public Vector2 posicao = new Vector2(0f, 150f);
    public Vector2 tamanhoPainel = new Vector2(760f, 84f);
    public int tamanhoFonte = 26;
    public Color corTexto = new Color(0.92f, 0.96f, 1f, 1f);
    public Color corFundo = new Color(0f, 0f, 0f, 0.62f);

    private GameObject painel;
    private TextMeshProUGUI texto;
    private Coroutine rotina;
    private bool jaFalou;

    private void Awake()
    {
        CriarLegendaSePrecisar();
        EsconderLegenda();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.name != nomeColliderDialogo)
            return;

        MostrarLegenda();
    }

    private void MostrarLegenda()
    {
        if (falarUmaVez && jaFalou)
            return;

        jaFalou = true;
        CriarLegendaSePrecisar();

        if (texto == null || painel == null)
            return;

        texto.text = fala;
        painel.SetActive(true);

        if (rotina != null)
            StopCoroutine(rotina);

        rotina = StartCoroutine(EsconderDepois());
    }

    private IEnumerator EsconderDepois()
    {
        yield return new WaitForSeconds(tempoNaTela);
        EsconderLegenda();
        rotina = null;
    }

    private void EsconderLegenda()
    {
        if (painel != null)
            painel.SetActive(false);
    }

    private void CriarLegendaSePrecisar()
    {
        if (painel != null && texto != null)
            return;

        GameObject canvasObj = new GameObject("CanvasLegendaCaverna");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        painel = new GameObject("PainelLegendaCaverna");
        painel.transform.SetParent(canvasObj.transform, false);

        Image fundo = painel.AddComponent<Image>();
        fundo.color = corFundo;

        RectTransform painelRect = painel.GetComponent<RectTransform>();
        painelRect.anchorMin = new Vector2(0.5f, 0f);
        painelRect.anchorMax = new Vector2(0.5f, 0f);
        painelRect.pivot = new Vector2(0.5f, 0.5f);
        painelRect.anchoredPosition = posicao;
        painelRect.sizeDelta = tamanhoPainel;

        GameObject textoObj = new GameObject("TextoLegendaCaverna");
        textoObj.transform.SetParent(painel.transform, false);
        texto = textoObj.AddComponent<TextMeshProUGUI>();
        texto.text = fala;
        texto.color = corTexto;
        texto.fontSize = tamanhoFonte;
        texto.alignment = TextAlignmentOptions.Center;
        texto.textWrappingMode = TextWrappingModes.Normal;
        texto.raycastTarget = false;

        RectTransform textoRect = texto.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(24f, 8f);
        textoRect.offsetMax = new Vector2(-24f, -8f);
    }
}
