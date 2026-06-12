using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileJoystickControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform miolo;
    public Image imagemBase;
    public float raio = 52f;
    public Color corNormal;
    public Color corPressionado;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (imagemBase != null)
            imagemBase.color = corPressionado;

        Atualizar(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Atualizar(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MobileControlsInput.DefinirHorizontal(0f);

        if (miolo != null)
            miolo.anchoredPosition = Vector2.zero;

        if (imagemBase != null)
            imagemBase.color = corNormal;
    }

    private void Atualizar(PointerEventData eventData)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 local);
        Vector2 limitado = Vector2.ClampMagnitude(local, raio);

        if (miolo != null)
            miolo.anchoredPosition = limitado;

        MobileControlsInput.DefinirHorizontal(limitado.x / raio);
    }
}
