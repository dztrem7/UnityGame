using UnityEngine;

public class FundoInfinito : MonoBehaviour
{
    public Transform cameraTransform;

    [Range(0f, 1f)]
    public float parallax = 0.5f;
    public bool pararNoObjetoPararParalax = true;
    public string nomeObjetoPararParalax = "PararParalax";

    private float posicaoInicialX;
    private Transform player;
    private Transform pararParalax;
    private bool parallaxParado;

    void Start()
    {
        posicaoInicialX = transform.position.x;
        EncontrarReferencias();
    }

    void Update()
    {
        if (parallaxParado)
            return;

        if (DevePararParalax())
        {
            parallaxParado = true;
            return;
        }

        if (cameraTransform == null)
            return;

        float novaPosX = posicaoInicialX + (cameraTransform.position.x * parallax);

        transform.position = new Vector3(
            novaPosX,
            transform.position.y,
            transform.position.z
        );
    }

    private bool DevePararParalax()
    {
        if (!pararNoObjetoPararParalax)
            return false;

        if (player == null || pararParalax == null)
            EncontrarReferencias();

        return player != null && pararParalax != null && player.position.x >= pararParalax.position.x;
    }

    private void EncontrarReferencias()
    {
        if (player == null)
        {
            try
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("player");
                if (playerObj != null)
                    player = playerObj.transform;
            }
            catch (UnityException)
            {
            }
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.Find("bonecoPrincipal");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (pararParalax == null)
        {
            GameObject pararObj = GameObject.Find(nomeObjetoPararParalax);
            if (pararObj != null)
                pararParalax = pararObj.transform;
        }
    }
}
