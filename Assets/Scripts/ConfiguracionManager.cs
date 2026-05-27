// Gestiona la pantalla de configuración del juego. Proporciona control de volumen mediante AudioListener,
// las acciones de reiniciar progreso y eliminar cuenta (con confirmación previa compartida) y el botón de salir de la aplicación.
// El volumen se persiste en PlayerPrefs entre sesiones.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfiguracionManager : MonoBehaviour
{
    /// <summary>Instancia única accesible desde cualquier script.</summary>
    public static ConfiguracionManager Instance;

    [Header("Paneles")]
    public GameObject panelConfiguracion; // Panel principal de configuración
    public GameObject panelConfirmacion;  // Diálogo de confirmación compartido (reset y borrar cuenta)

    [Header("Volumen")]
    public Slider sliderVolumen; // Slider que controla AudioListener.volume (rango 0-1)

    [Header("Confirmación")]
    public TextMeshProUGUI txtConfirmacion; // Texto del diálogo de confirmación (varía según la acción)

    /// <summary>
    /// Flag que indica qué acción está pendiente de confirmación:
    /// true = eliminar cuenta, false = reiniciar progreso.
    /// </summary>
    private bool esperandoEliminar = false;

    //  Ciclo de vida Unity 

    void Awake() { Instance = this; }

    void Start()
    {
        // Restaurar el volumen guardado en la sesión anterior
        float volumen = PlayerPrefs.GetFloat("volumen", 1f);
        AudioListener.volume = volumen;

        // Sincronizar el slider sin disparar el evento OnValueChanged
        if (sliderVolumen != null)
            sliderVolumen.SetValueWithoutNotify(volumen);
    }

    //  Abrir / Cerrar 

    /// <summary>Abre el panel de configuración cerrando cualquier otro panel abierto.</summary>
    public void AbrirConfiguracion()
    {
        TiendaManager.Instance.CerrarTienda();
        MejorasManager.Instance.CerrarMejoras();
        PerfilManager.Instance.CerrarPerfil();
        if (RankingManager.Instance != null) RankingManager.Instance.CerrarRanking();
        panelConfiguracion.SetActive(true);
        panelConfirmacion.SetActive(false);
    }

    /// <summary>Cierra el panel de configuración y el de confirmación si estuviera abierto.</summary>
    public void CerrarConfiguracion()
    {
        panelConfiguracion.SetActive(false);
        panelConfirmacion.SetActive(false);
    }

    //  Control de volumen 

    /// <summary>
    /// Ajusta el volumen global del juego en tiempo real y lo persiste en PlayerPrefs.
    /// Se conecta al evento OnValueChanged (Dynamic float) del Slider en el Inspector.
    /// </summary>
    /// <param name="valor">Valor del slider entre 0 (silencio) y 1 (máximo volumen).</param>
    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat("volumen", valor);
        PlayerPrefs.Save();
    }

    //  Confirmación de acciones destructivas 

    /// <summary>
    /// Muestra el diálogo de confirmación para reiniciar el progreso,
    /// indicando que se perderán todos los avances.
    /// </summary>
    public void MostrarConfirmacionReset()
    {
        esperandoEliminar = false;
        if (txtConfirmacion != null)
            txtConfirmacion.text = "¿Seguro que quieres reiniciar el progreso?\nPerderas todo tu dinero, parcelas y mejoras.";
        panelConfirmacion.SetActive(true);
    }

    /// <summary>
    /// Muestra el diálogo de confirmación para eliminar la cuenta,
    /// advirtiendo de que la acción es irreversible.
    /// </summary>
    public void MostrarConfirmacionEliminar()
    {
        esperandoEliminar = true;
        if (txtConfirmacion != null)
            txtConfirmacion.text = "¿Seguro que quieres eliminar tu cuenta?\nEsta accion no se puede deshacer.";
        panelConfirmacion.SetActive(true);
    }

    /// <summary>
    /// Ejecuta la acción confirmada por el usuario: reiniciar progreso o eliminar cuenta,
    /// según el valor del flag <see cref="esperandoEliminar"/>.
    /// </summary>
    public void Confirmar()
    {
        panelConfirmacion.SetActive(false);
        if (esperandoEliminar)
            PerfilManager.Instance.EliminarCuenta();
        else
        {
            CerrarConfiguracion();
            PerfilManager.Instance.ReiniciarProgreso();
        }
    }

    /// <summary>Cancela la acción pendiente y cierra el diálogo de confirmación.</summary>
    public void Cancelar()
    {
        panelConfirmacion.SetActive(false);
    }

    //  Salir del juego 

    /// <summary>
    /// Cierra la aplicación. En el editor de Unity este método no tiene efecto;
    /// solo funciona en builds compilados.
    /// </summary>
    public void SalirJuego()
    {
        Application.Quit();
    }
}
