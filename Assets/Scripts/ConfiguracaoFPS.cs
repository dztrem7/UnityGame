using UnityEngine;

public static class ConfiguracaoFPS
{
    public const string ChaveFPS = "OpcaoFPS";
    public const int FPSPadrao = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AplicarAoAbrirJogo()
    {
        AplicarFPS(PlayerPrefs.GetInt(ChaveFPS, FPSPadrao), false);
    }

    public static int FPSAtualSalvo()
    {
        return PlayerPrefs.GetInt(ChaveFPS, FPSPadrao);
    }

    public static void AplicarFPS(int fps, bool salvar)
    {
        fps = NormalizarFPS(fps);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;

        if (!salvar)
            return;

        PlayerPrefs.SetInt(ChaveFPS, fps);
        PlayerPrefs.Save();
    }

    private static int NormalizarFPS(int fps)
    {
        if (fps <= 30)
            return 30;

        if (fps <= 60)
            return 60;

        return 120;
    }
}
