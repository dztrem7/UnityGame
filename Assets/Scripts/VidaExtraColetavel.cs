using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Collider2D))]
public class VidaExtraColetavel : MonoBehaviour
{
    public int quantidadeVidaExtra = 1;
    public bool destruirAoColetar = true;
    public AudioClip somColetar;
    [Range(0f, 1f)] public float volumeSom = 0.85f;
    public Color corCoracao = new Color(0.02f, 0.02f, 0.025f, 1f);
    public Color corBrilho = new Color(0.85f, 0.95f, 1f, 0.35f);
    public bool usarTamanhoVisualAutomatico = false;
    public Vector2 tamanhoVisual = new Vector2(0.55f, 0.5f);

    private SpriteRenderer spriteRenderer;

    private void Reset()
    {
        ConfigurarCollider();
        ConfigurarVisual();
    }

    private void Awake()
    {
        ConfigurarCollider();
        ConfigurarVisual();
    }

    private void OnEnable()
    {
        ConfigurarCollider();
        ConfigurarVisual();
    }

    private void OnValidate()
    {
        ConfigurarCollider();
        ConfigurarVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Vida vida = other.GetComponentInParent<Vida>();

        if (vida == null)
            return;

        vida.GanharVidaExtra(quantidadeVidaExtra);
        TocarSom();

        if (destruirAoColetar)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void ConfigurarCollider()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.isTrigger = true;
    }

    private void ConfigurarVisual()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (spriteRenderer.sprite == null)
            spriteRenderer.sprite = CriarSpriteCoracao(96);

        spriteRenderer.color = Color.white;

        if (usarTamanhoVisualAutomatico)
            transform.localScale = new Vector3(tamanhoVisual.x, tamanhoVisual.y, 1f);
    }

    private void TocarSom()
    {
        if (somColetar == null)
            return;

        AudioSource.PlayClipAtPoint(somColetar, transform.position, volumeSom);
    }

    private Sprite CriarSpriteCoracao(int tamanho)
    {
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point;

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                Vector2 uv = new Vector2((x + 0.5f) / tamanho, (y + 0.5f) / tamanho);
                float corpo = ShapeCoracao(uv);
                float brilho = ShapeBrilho(uv);
                Color cor = Color.clear;

                if (corpo > 0.52f)
                    cor = corCoracao;

                if (brilho > 0.72f && corpo > 0.52f)
                    cor = Color.Lerp(cor, corBrilho, 0.45f);

                textura.SetPixel(x, y, cor);
            }
        }

        textura.Apply(false);
        return Sprite.Create(textura, new Rect(0f, 0f, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100f);
    }

    private float ShapeCoracao(Vector2 uv)
    {
        float esquerdo = Ellipse(uv, new Vector2(0.36f, 0.62f), new Vector2(0.22f, 0.2f));
        float direito = Ellipse(uv, new Vector2(0.64f, 0.62f), new Vector2(0.22f, 0.2f));
        float baseCoracao = Losango(uv, new Vector2(0.5f, 0.39f), new Vector2(0.38f, 0.34f));
        return Mathf.Max(Mathf.Max(esquerdo, direito), baseCoracao);
    }

    private float ShapeBrilho(Vector2 uv)
    {
        return Ellipse(uv, new Vector2(0.38f, 0.66f), new Vector2(0.07f, 0.04f));
    }

    private float Ellipse(Vector2 uv, Vector2 centro, Vector2 raio)
    {
        Vector2 d = new Vector2((uv.x - centro.x) / raio.x, (uv.y - centro.y) / raio.y);
        return Mathf.Clamp01(1f - d.sqrMagnitude);
    }

    private float Losango(Vector2 uv, Vector2 centro, Vector2 raio)
    {
        Vector2 d = new Vector2(Mathf.Abs(uv.x - centro.x) / raio.x, Mathf.Abs(uv.y - centro.y) / raio.y);
        return Mathf.Clamp01(1f - d.x - d.y);
    }
}
