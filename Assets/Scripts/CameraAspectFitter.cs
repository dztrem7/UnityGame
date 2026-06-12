using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectFitter : MonoBehaviour
{
    public float aspectoReferencia = 16f / 9f;
    public float tamanhoOrtograficoReferencia = 8f;
    public bool manterLarguraDoPC = true;
    public bool forcarLandscape = true;
    public bool corrigirViewport = true;

    private Camera cameraAlvo;
    private int ultimaLargura;
    private int ultimaAltura;

    private void Awake()
    {
        cameraAlvo = GetComponent<Camera>();
        AplicarAjuste();
    }

    private void Start()
    {
        AplicarAjuste();
    }

    private void LateUpdate()
    {
        if (ultimaLargura == Screen.width && ultimaAltura == Screen.height)
            return;

        AplicarAjuste();
    }

    private void AplicarAjuste()
    {
        ultimaLargura = Screen.width;
        ultimaAltura = Screen.height;

        if (forcarLandscape)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        if (cameraAlvo == null)
            cameraAlvo = GetComponent<Camera>();

        if (cameraAlvo == null)
            return;

        if (corrigirViewport)
            cameraAlvo.rect = new Rect(0f, 0f, 1f, 1f);

        if (!manterLarguraDoPC || !cameraAlvo.orthographic)
            return;

        float aspectoAtual = Screen.height > 0 ? (float)Screen.width / Screen.height : cameraAlvo.aspect;

        if (aspectoAtual <= 0f)
            aspectoAtual = aspectoReferencia;

        cameraAlvo.orthographicSize = tamanhoOrtograficoReferencia * (aspectoReferencia / aspectoAtual);
    }
}
