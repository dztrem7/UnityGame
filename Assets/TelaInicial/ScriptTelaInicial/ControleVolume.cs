using UnityEngine;
using UnityEngine.UI;

public class ControleVolume : MonoBehaviour
{
    public Slider sliderVolume;
    public AudioSource musica;

    void Start()
    {
        sliderVolume.value = musica.volume;

        sliderVolume.onValueChanged.AddListener(MudarVolume);
    }

    public void MudarVolume(float valor)
    {
        musica.volume = valor;
    }
}