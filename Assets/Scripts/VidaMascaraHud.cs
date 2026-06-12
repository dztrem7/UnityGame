using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class VidaMascaraHud : MonoBehaviour
{
    public bool esconderCoracoesAntigos = true;
    public int quantidadePadraoEditor = 6;
    public Vector2 margem = new Vector2(44f, 36f);
    public Vector2 tamanhoMascara = new Vector2(34f, 42f);
    public float espacamento = 39f;
    public Color corCheia = new Color(0.93f, 0.98f, 1f, 0.96f);
    public Color corVazia = new Color(0f, 0f, 0f, 0.62f);
    public Color corBorda = new Color(0.78f, 0.9f, 1f, 0.58f);
    public Color corSombra = new Color(0f, 0f, 0f, 0.82f);
    public bool usarPosicaoDaUnity = true;
    public bool animarPerdaVida = true;
    public float duracaoAnimacaoPerda = 0.38f;
    public float quedaPerdaVida = 18f;
    public float escalaPerdaVida = 0.78f;

    private RectTransform rectTransform;
    private RectTransform grupo;
    private Image[] mascarasCheias;
    private Image[] mascarasVazias;
    private RectTransform[] rectsCheios;
    private Vector2[] posicoesOriginais;
    private Vector3[] escalasOriginais;
    private Coroutine[] animacoesPerda;
    private Sprite spriteCheio;
    private Sprite spriteVazio;
    private Sprite spriteSombra;
    private int quantidadeAtual;
    private float ultimaVida = -1f;

    private void OnEnable()
    {
        Configurar(quantidadePadraoEditor);
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Configurar(quantidadePadraoEditor);
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;

        quantidadePadraoEditor = Mathf.Max(1, quantidadePadraoEditor);
        Configurar(quantidadePadraoEditor);
    }

    public void Configurar(int quantidade)
    {
        if (quantidade <= 0)
            return;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (quantidadeAtual == quantidade && grupo != null && grupo.childCount >= quantidade * 3)
            return;

        quantidadeAtual = quantidade;
        CriarSprites();
        CriarGrupo();
        CriarMascaras();
    }

    public void Atualizar(float vidaAtual, int vidaMaxima)
    {
        Configurar(vidaMaxima);

        if (mascarasCheias == null)
            return;

        for (int i = 0; i < mascarasCheias.Length; i++)
        {
            bool cheio = vidaAtual >= i + 1;
            bool estavaCheio = ultimaVida < 0f || ultimaVida >= i + 1;

            if (!cheio && estavaCheio && animarPerdaVida && Application.isPlaying)
                TocarAnimacaoPerda(i);
            else if (!cheio)
                mascarasCheias[i].enabled = false;
            else
                RestaurarMascaraCheia(i);
        }

        ultimaVida = vidaAtual;
    }

    public void EsconderCoracoes(Image[] coracoes)
    {
        if (!esconderCoracoesAntigos || coracoes == null)
            return;

        foreach (Image coracao in coracoes)
        {
            if (coracao != null)
                coracao.gameObject.SetActive(false);
        }

        Image[] imagens = GetComponentsInChildren<Image>(true);

        foreach (Image imagem in imagens)
        {
            if (imagem == null || imagem.transform.IsChildOf(grupo))
                continue;

            if (imagem.name.StartsWith("Coracao"))
                imagem.gameObject.SetActive(false);
        }
    }

    private void CriarGrupo()
    {
        Transform existente = transform.Find("VidaMascaraHUD");
        GameObject grupoObj = existente != null ? existente.gameObject : new GameObject("VidaMascaraHUD");
        grupoObj.transform.SetParent(transform, false);

        grupo = grupoObj.GetComponent<RectTransform>();

        if (grupo == null)
            grupo = grupoObj.AddComponent<RectTransform>();

        if (!usarPosicaoDaUnity || existente == null)
        {
            grupo.anchorMin = new Vector2(0f, 1f);
            grupo.anchorMax = new Vector2(0f, 1f);
            grupo.pivot = new Vector2(0f, 1f);
            grupo.anchoredPosition = new Vector2(margem.x, -margem.y);
            grupo.sizeDelta = new Vector2((quantidadeAtual - 1) * espacamento + tamanhoMascara.x, tamanhoMascara.y);
        }

        if (!usarPosicaoDaUnity)
        {
            for (int i = grupo.childCount - 1; i >= 0; i--)
                DestruirObjeto(grupo.GetChild(i).gameObject);
        }
    }

    private void CriarMascaras()
    {
        mascarasCheias = new Image[quantidadeAtual];
        mascarasVazias = new Image[quantidadeAtual];
        rectsCheios = new RectTransform[quantidadeAtual];
        posicoesOriginais = new Vector2[quantidadeAtual];
        escalasOriginais = new Vector3[quantidadeAtual];
        animacoesPerda = new Coroutine[quantidadeAtual];

        for (int i = 0; i < quantidadeAtual; i++)
        {
            Vector2 posicao = new Vector2(i * espacamento, 0f);

            CriarOuAtualizarImagem("SombraMascara " + i, spriteSombra, corSombra, posicao + new Vector2(2f, -3f));
            mascarasVazias[i] = CriarOuAtualizarImagem("MascaraVazia " + i, spriteVazio, corBorda, posicao);
            mascarasCheias[i] = CriarOuAtualizarImagem("MascaraCheia " + i, spriteCheio, corCheia, posicao);
            rectsCheios[i] = mascarasCheias[i].GetComponent<RectTransform>();
            posicoesOriginais[i] = rectsCheios[i].anchoredPosition;
            escalasOriginais[i] = rectsCheios[i].localScale;
        }
    }

    private void TocarAnimacaoPerda(int indice)
    {
        if (indice < 0 || indice >= mascarasCheias.Length || mascarasCheias[indice] == null)
            return;

        if (animacoesPerda[indice] != null)
            StopCoroutine(animacoesPerda[indice]);

        animacoesPerda[indice] = StartCoroutine(AnimarPerda(indice));
    }

    private System.Collections.IEnumerator AnimarPerda(int indice)
    {
        Image imagem = mascarasCheias[indice];
        RectTransform rect = rectsCheios[indice];

        if (imagem == null || rect == null)
            yield break;

        imagem.enabled = true;
        imagem.color = corCheia;

        Vector2 inicioPos = posicoesOriginais[indice];
        Vector2 fimPos = inicioPos + Vector2.down * quedaPerdaVida;
        Vector3 inicioEscala = escalasOriginais[indice];
        Vector3 fimEscala = inicioEscala * escalaPerdaVida;
        float tempo = 0f;

        while (tempo < duracaoAnimacaoPerda)
        {
            float progresso = tempo / duracaoAnimacaoPerda;
            float suavizado = Mathf.SmoothStep(0f, 1f, progresso);
            Color cor = corCheia;
            cor.a = Mathf.Lerp(corCheia.a, 0f, suavizado);

            rect.anchoredPosition = Vector2.Lerp(inicioPos, fimPos, suavizado);
            rect.localScale = Vector3.Lerp(inicioEscala, fimEscala, suavizado);
            imagem.color = cor;

            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        imagem.enabled = false;
        rect.anchoredPosition = inicioPos;
        rect.localScale = inicioEscala;
        imagem.color = corCheia;
        animacoesPerda[indice] = null;
    }

    private void RestaurarMascaraCheia(int indice)
    {
        if (indice < 0 || indice >= mascarasCheias.Length || mascarasCheias[indice] == null)
            return;

        if (animacoesPerda != null && animacoesPerda[indice] != null)
        {
            StopCoroutine(animacoesPerda[indice]);
            animacoesPerda[indice] = null;
        }

        mascarasCheias[indice].enabled = true;
        mascarasCheias[indice].color = corCheia;

        if (rectsCheios != null && rectsCheios[indice] != null)
        {
            rectsCheios[indice].anchoredPosition = posicoesOriginais[indice];
            rectsCheios[indice].localScale = escalasOriginais[indice];
        }
    }

    private Image CriarOuAtualizarImagem(string nome, Sprite sprite, Color cor, Vector2 posicao)
    {
        Transform existente = grupo.Find(nome);
        GameObject obj = existente != null ? existente.gameObject : new GameObject(nome);
        obj.transform.SetParent(grupo, false);

        Image imagem = obj.GetComponent<Image>();

        if (imagem == null)
            imagem = obj.AddComponent<Image>();

        imagem.sprite = sprite;
        imagem.color = cor;
        imagem.raycastTarget = false;
        imagem.preserveAspect = true;

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null)
            rect = obj.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        if (!usarPosicaoDaUnity || existente == null)
        {
            rect.anchoredPosition = posicao;
            rect.sizeDelta = tamanhoMascara;
        }

        return imagem;
    }

    private void DestruirObjeto(GameObject obj)
    {
        if (obj == null)
            return;

        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }

    private void CriarSprites()
    {
        if (spriteCheio != null)
            return;

        spriteCheio = CriarSpriteMascara(96, true);
        spriteVazio = CriarSpriteMascara(96, false);
        spriteSombra = CriarSpriteSombra(96);
    }

    private Sprite CriarSpriteMascara(int tamanho, bool preenchido)
    {
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                Vector2 uv = new Vector2((x + 0.5f) / tamanho, (y + 0.5f) / tamanho);
                float mascara = ShapeMascara(uv);
                float olhoEsq = ShapeOlho(uv, new Vector2(0.38f, 0.57f));
                float olhoDir = ShapeOlho(uv, new Vector2(0.62f, 0.57f));
                bool borda = mascara > 0.42f && mascara < 0.62f;
                bool dentro = mascara >= 0.62f;
                Color cor = Color.clear;

                if (preenchido && dentro)
                    cor = Color.white;
                else if (!preenchido && borda)
                    cor = Color.white;

                if ((preenchido || borda) && (olhoEsq > 0.66f || olhoDir > 0.66f))
                    cor = Color.clear;

                textura.SetPixel(x, y, cor);
            }
        }

        textura.Apply(false);
        return Sprite.Create(textura, new Rect(0f, 0f, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CriarSpriteSombra(int tamanho)
    {
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                Vector2 uv = new Vector2((x + 0.5f) / tamanho, (y + 0.5f) / tamanho);
                float mascara = ShapeMascara(uv);
                textura.SetPixel(x, y, mascara > 0.44f ? Color.white : Color.clear);
            }
        }

        textura.Apply(false);
        return Sprite.Create(textura, new Rect(0f, 0f, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100f);
    }

    private float ShapeMascara(Vector2 uv)
    {
        float cabeca = Ellipse(uv, new Vector2(0.5f, 0.54f), new Vector2(0.34f, 0.31f));
        float queixo = Ellipse(uv, new Vector2(0.5f, 0.36f), new Vector2(0.25f, 0.22f));
        float chifreEsq = Ellipse(Rotacionar(uv, new Vector2(0.33f, 0.76f), -30f), new Vector2(0.33f, 0.76f), new Vector2(0.1f, 0.24f));
        float chifreDir = Ellipse(Rotacionar(uv, new Vector2(0.67f, 0.76f), 30f), new Vector2(0.67f, 0.76f), new Vector2(0.1f, 0.24f));
        return Mathf.Max(Mathf.Max(cabeca, queixo), Mathf.Max(chifreEsq, chifreDir));
    }

    private float ShapeOlho(Vector2 uv, Vector2 centro)
    {
        return Ellipse(uv, centro, new Vector2(0.075f, 0.1f));
    }

    private float Ellipse(Vector2 uv, Vector2 centro, Vector2 raio)
    {
        Vector2 d = new Vector2((uv.x - centro.x) / raio.x, (uv.y - centro.y) / raio.y);
        return Mathf.Clamp01(1f - (d.x * d.x + d.y * d.y));
    }

    private Vector2 Rotacionar(Vector2 uv, Vector2 centro, float graus)
    {
        float rad = graus * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        Vector2 d = uv - centro;
        return new Vector2(d.x * cos - d.y * sin, d.x * sin + d.y * cos) + centro;
    }
}
