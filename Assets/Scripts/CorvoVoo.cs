using UnityEngine;

public class CorvoVoo : MonoBehaviour
{
    public float velocidade = 5f;
    public Vector3 destino = new Vector3(-39.14f, 1.32f, 0f);

    private bool voando = false;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (voando)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidade * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, destino) < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    public void Voar()
    {
        voando = true;

        if (anim != null)
        {
            anim.SetTrigger("CorvoVoo"); // 🔥 nome da animação
        }
    }
}