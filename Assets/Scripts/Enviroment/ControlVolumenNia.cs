/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Período: 2.º cuatrimestre de 2026
 * Materia: Programación 2
 * Programador: Eduardo Ortega
 * Función: Ajusta por separado el volumen del ambiente y de los efectos
 * de sonido en FMOD mediante dos deslizadores. Guarda las preferencias
 * con PlayerPrefs y las recupera cuando FMOD termina de cargar.
 *********************************************************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ControlVolumenNia : MonoBehaviour
{
    [SerializeField] private Slider sliderAmbiente;
    [SerializeField] private Slider sliderFX;

    private FMOD.Studio.Bus busAmbiente;
    private FMOD.Studio.Bus busFX;

    private IEnumerator Start()
    {
        yield return new WaitUntil(
            () => FMODUnity.RuntimeManager.HaveMasterBanksLoaded
        );

        busAmbiente = FMODUnity.RuntimeManager.GetBus("bus:/AmbientGroup");
        busFX = FMODUnity.RuntimeManager.GetBus("bus:/Fx");

        float ambiente = PlayerPrefs.GetFloat("VolumenAmbiente", 0.7f);
        float efectos = PlayerPrefs.GetFloat("VolumenFX", 1f);

        sliderAmbiente.SetValueWithoutNotify(ambiente);
        sliderFX.SetValueWithoutNotify(efectos);

        CambiarAmbiente(ambiente);
        CambiarFX(efectos);

        sliderAmbiente.onValueChanged.AddListener(CambiarAmbiente);
        sliderFX.onValueChanged.AddListener(CambiarFX);
    }

    private void CambiarAmbiente(float valor)
    {
        valor = Mathf.Clamp01(valor);
        busAmbiente.setVolume(valor);
        PlayerPrefs.SetFloat("VolumenAmbiente", valor);
    }

    private void CambiarFX(float valor)
    {
        valor = Mathf.Clamp01(valor);
        busFX.setVolume(valor);
        PlayerPrefs.SetFloat("VolumenFX", valor);
    }

    private void OnDestroy()
    {
        sliderAmbiente.onValueChanged.RemoveListener(CambiarAmbiente);
        sliderFX.onValueChanged.RemoveListener(CambiarFX);
        PlayerPrefs.Save();
    }
}