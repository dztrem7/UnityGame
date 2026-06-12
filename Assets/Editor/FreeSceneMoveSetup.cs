#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FreeSceneMoveSetup
{
    static FreeSceneMoveSetup()
    {
        EditorApplication.delayCall += AplicarMovimentoLivre;
    }

    [MenuItem("Tools/Cenario/Desligar Snap e Liberar Movimento")]
    public static void AplicarMovimentoLivre()
    {
        EditorSnapSettings.move = new Vector3(0.01f, 0.01f, 0.01f);
        EditorSnapSettings.rotate = 1f;
        EditorSnapSettings.scale = 0.01f;

        DefinirBoolEditorSnap("gridSnapEnabled", false);
        DefinirBoolEditorSnap("incrementalSnapActive", false);

        SceneView.RepaintAll();
        Debug.Log("Snap desligado: objetos do cenario podem ser movidos com controle fino.");
    }

    private static void DefinirBoolEditorSnap(string nomePropriedade, bool valor)
    {
        PropertyInfo propriedade = typeof(EditorSnapSettings).GetProperty(
            nomePropriedade,
            BindingFlags.Public | BindingFlags.Static);

        if (propriedade != null && propriedade.CanWrite && propriedade.PropertyType == typeof(bool))
            propriedade.SetValue(null, valor);
    }
}
#endif
