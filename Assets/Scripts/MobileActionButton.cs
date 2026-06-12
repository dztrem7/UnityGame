using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Acao
    {
        Pular,
        Atacar,
        Interagir,
        EscaparGrude
    }

    public Acao acao;
    public Image imagem;
    public Color corNormal = Color.white;
    public Color corPressionado = Color.white;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (acao == Acao.Atacar)
            MobileControlsInput.PressionarAtaque();
        else if (acao == Acao.Interagir)
            MobileControlsInput.PressionarInteracao();
        else if (acao == Acao.EscaparGrude)
            MobileControlsInput.PressionarEscaparGrude();
        else
            MobileControlsInput.PressionarPulo();

        if (imagem != null)
            imagem.color = corPressionado;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (imagem != null)
            imagem.color = corNormal;
    }
}
