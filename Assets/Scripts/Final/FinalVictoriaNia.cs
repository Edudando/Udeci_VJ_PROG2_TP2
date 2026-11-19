using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ParticleSystem))]
public sealed class FinalVictoriaNia : MonoBehaviour
{
    [SerializeField] private string escenaMenuPrincipal;
    [SerializeField] private FMODUnity.StudioEventEmitter audioFuegos;
    [SerializeField, Min(0f)] private float esperaTrasColorear = 0.8f;
    [SerializeField, Min(0f)] private float separacionEntreEstallidos = 0.45f;
    [SerializeField, Min(0.1f)] private float duracionChispas = 1.45f;
    [SerializeField, Min(0f)] private float separacionLateral = 1.8f;
    [SerializeField, Min(0.01f)] private float tamanoChispas = 0.12f;
    [SerializeField, Min(0.1f)] private float velocidadChispas = 3f;
    [SerializeField] private Behaviour[] controlesDuranteElFinal = new Behaviour[0];

    private ParticleSystem fuegos;
    private bool victoriaIniciada;

    private void Awake()
    {
        fuegos = GetComponent<ParticleSystem>();
        fuegos.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

        var ajustes = fuegos.main;
        ajustes.playOnAwake = false;
        ajustes.loop = false;
        ajustes.duration = 8f;
        ajustes.simulationSpace = ParticleSystemSimulationSpace.World;
        ajustes.useUnscaledTime = true;
        ajustes.gravityModifier = 0.2f;
        ajustes.maxParticles = 300;

        var emision = fuegos.emission;
        emision.enabled = false;

        var forma = fuegos.shape;
        forma.enabled = false;

        var color = fuegos.colorOverLifetime;
        color.enabled = true;
        var desvanecido = new Gradient();
        desvanecido.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.35f), new GradientAlphaKey(0f, 1f) });
        color.color = new ParticleSystem.MinMaxGradient(desvanecido);
    }

    public void IniciarVictoria()
    {
        if (victoriaIniciada || !isActiveAndEnabled)
            return;

        if (string.IsNullOrWhiteSpace(escenaMenuPrincipal) ||
            !Application.CanStreamedLevelBeLoaded(escenaMenuPrincipal))
        {
            Debug.LogError("Indicá una escena de menú habilitada en la Scene List del build.", this);
            return;
        }

        victoriaIniciada = true;
        StartCoroutine(CelebrarYVolverAlMenu());
    }

    private IEnumerator CelebrarYVolverAlMenu()
    {
        foreach (Behaviour control in controlesDuranteElFinal)
        {
            if (control != null && control != this)
                control.enabled = false;
        }

        yield return new WaitForSecondsRealtime(esperaTrasColorear);

        fuegos.Play(false);
        audioFuegos?.Play();
        
        Vector3 lateral = transform.right * separacionLateral;
        EmitirEstallido(transform.position - lateral, new Color(1f, 0.72f, 0.17f));
        yield return new WaitForSecondsRealtime(separacionEntreEstallidos);
        EmitirEstallido(transform.position + lateral, new Color(0.2f, 0.85f, 1f));
        yield return new WaitForSecondsRealtime(separacionEntreEstallidos);
        EmitirEstallido(transform.position + Vector3.up * 0.65f, new Color(1f, 0.37f, 0.74f));
        yield return new WaitForSecondsRealtime(duracionChispas + 0.15f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return SceneManager.LoadSceneAsync(escenaMenuPrincipal);
    }

    private void EmitirEstallido(Vector3 centro, Color color)
    {
        const int cantidadChispas = 65;
        for (int i = 0; i < cantidadChispas; i++)
        {
            fuegos.Emit(new ParticleSystem.EmitParams
            {
                position = centro,
                velocity = Random.onUnitSphere * velocidadChispas * Random.Range(0.6f, 1f),
                startLifetime = duracionChispas * Random.Range(0.7f, 1f),
                startSize = tamanoChispas * Random.Range(0.75f, 1.25f),
                startColor = color
            }, 1);
        }
    }
}
