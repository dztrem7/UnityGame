public static class MobileControlsInput
{
    private static bool puloPressionado;
    private static bool ataquePressionado;
    private static bool interacaoPressionada;
    private static bool escaparGrudePressionado;

    public static float Horizontal { get; private set; }
    public static bool InteracaoDisponivel { get; private set; }
    public static bool EscaparGrudeDisponivel { get; private set; }

    public static void DefinirHorizontal(float valor)
    {
        Horizontal = UnityEngine.Mathf.Clamp(valor, -1f, 1f);
    }

    public static void PressionarPulo()
    {
        puloPressionado = true;
    }

    public static void PressionarAtaque()
    {
        ataquePressionado = true;
    }

    public static void PressionarInteracao()
    {
        interacaoPressionada = true;
    }

    public static void PressionarEscaparGrude()
    {
        escaparGrudePressionado = true;
    }

    public static void DefinirInteracaoDisponivel(bool disponivel)
    {
        InteracaoDisponivel = disponivel;

        if (!disponivel)
            interacaoPressionada = false;
    }

    public static void DefinirEscaparGrudeDisponivel(bool disponivel)
    {
        EscaparGrudeDisponivel = disponivel;

        if (!disponivel)
            escaparGrudePressionado = false;
    }

    public static bool ConsumirPulo()
    {
        if (!puloPressionado)
            return false;

        puloPressionado = false;
        return true;
    }

    public static bool ConsumirAtaque()
    {
        if (!ataquePressionado)
            return false;

        ataquePressionado = false;
        return true;
    }

    public static bool ConsumirInteracao()
    {
        if (!interacaoPressionada)
            return false;

        interacaoPressionada = false;
        return true;
    }

    public static bool ConsumirEscaparGrude()
    {
        if (!escaparGrudePressionado)
            return false;

        escaparGrudePressionado = false;
        return true;
    }

    public static void Limpar()
    {
        Horizontal = 0f;
        puloPressionado = false;
        ataquePressionado = false;
        interacaoPressionada = false;
        escaparGrudePressionado = false;
        InteracaoDisponivel = false;
        EscaparGrudeDisponivel = false;
    }
}
