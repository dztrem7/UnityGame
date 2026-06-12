using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //camera
    public Transform player;
    public Vector3 offset = new Vector3(0f, 0.96f, -10f);
    public Vector3 ajusteMobile = new Vector3(3.6f, 0f, 0f);
    public Vector3 offsetMobile = new Vector3(12f, -0.3f, -10f);
    public float smoothSpeed = 0.125f;
    public bool usarPosicaoDaCenaComoOffset = false;
    public bool centralizarNoPlayerAoIniciar = false;
    public bool usarAjusteMobile = true;
    public bool usarOffsetMobileSeparado = true;
    public bool detectarMobilePorAspectoNoEditor = false;
    public float aspectoMinimoCelularWide = 1.85f;
    public float aspectoMaximoTablet = 1.55f;

    private Vector3 offsetDaCenaPC;
    private bool temOffsetDaCenaPC;

    private void Start()
    {
        if (player == null)
            return;

        if (usarPosicaoDaCenaComoOffset)
        {
            offsetDaCenaPC = transform.position - player.position;
            temOffsetDaCenaPC = true;
        }
        else if (centralizarNoPlayerAoIniciar)
            transform.position = player.position + offset;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 desiredPosition = player.position + ObterOffsetFinal();
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    private Vector3 ObterOffsetFinal()
    {
        if (EhTelaMobile())
        {
            if (usarOffsetMobileSeparado)
                return offsetMobile;

            if (usarAjusteMobile)
                return offset + ajusteMobile;
        }

        if (temOffsetDaCenaPC)
            return offsetDaCenaPC;

        return offset;
    }
//teste
    private bool EhTelaMobile()
    {
        float aspecto = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
        bool aspectoMobile = detectarMobilePorAspectoNoEditor
            && (aspecto >= aspectoMinimoCelularWide || aspecto <= aspectoMaximoTablet);
        return Application.isMobilePlatform || aspectoMobile;
    }
}
