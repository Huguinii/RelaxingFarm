// Representa una parcela de cultivo o instalación productiva del juego.
// Cada parcela genera dinero automáticamente al completar sus ciclos de producción cuando está desbloqueada.
// La barra de progreso refleja el avance del ciclo actual en tiempo real.
// Las parcelas no compradas permanecen completamente invisibles hasta que el jugador las adquiere.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Parcela : MonoBehaviour
{
    //  Configuración del producto
    [Header("Configuración")]

    /// <summary>Identificador único del producto ("producto_0" a "producto_8").</summary>
    public string idProducto;

    /// <summary>Dinero generado al completar un ciclo de producción.</summary>
    public double dineroPerCiclo = 10;

    /// <summary>Duración de cada ciclo de producción en segundos.</summary>
    public float tiempoCiclo = 5f;

    /// <summary>Nivel de la parcela (0 = bloqueada, 1+ = activa).</summary>
    public int nivel = 0;

    //  Referencias de UI
    [Header("UI")]
    public Image           barraProgreso; // Barra que rellena el ciclo actual
    public TextMeshProUGUI textDinero;    // Texto informativo de la parcela

    //  Estado interno

    /// <summary>Tiempo transcurrido del ciclo actual en segundos.</summary>
    public float timerActual = 0f;

    /// <summary>Indica si la parcela está activa y produciendo dinero.</summary>
    public bool desbloqueada = false;

    /// <summary>CanvasGroup usado para ocultar/mostrar la parcela sin modificar el layout.</summary>
    private CanvasGroup canvasGroup;

    //  Ciclo de vida Unity

    void Awake()
    {
        // Obtener o crear el CanvasGroup para controlar la visibilidad
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Ocultar la parcela si aún no ha sido comprada
        ActualizarVisibilidad();
    }

    void Update()
    {
        // Las parcelas bloqueadas no producen ni actualizan su UI
        if (!desbloqueada) return;

        timerActual += Time.deltaTime;

        // Actualizar el relleno de la barra de progreso proporcionalmente al ciclo
        if (barraProgreso != null)
            barraProgreso.fillAmount = timerActual / tiempoCiclo;

        // Al completar el ciclo, reiniciar el timer y añadir el dinero producido
        if (timerActual >= tiempoCiclo)
        {
            timerActual = 0f;
            GameManager.Instance.AñadirDinero(dineroPerCiclo);
        }
    }

    //  Visibilidad

    /// <summary>
    /// Aplica la visibilidad correcta según el estado de desbloqueo:
    /// opaca e interactuable si está desbloqueada, invisible e inactiva si está bloqueada.
    /// Se llama desde Desbloquear(), Bloquear() y desde SaveManager al cargar la partida.
    /// </summary>
    public void ActualizarVisibilidad()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha          = desbloqueada ? 1f : 0f;
        canvasGroup.interactable   = desbloqueada;
        canvasGroup.blocksRaycasts = desbloqueada;
    }

    //  Desbloqueo / Bloqueo

    /// <summary>
    /// Activa la parcela para que empiece a producir dinero y la hace visible.
    /// Se llama al comprar la parcela en la tienda o al cargar una partida guardada.
    /// </summary>
    public void Desbloquear()
    {
        desbloqueada = true;
        nivel        = 1;
        ActualizarVisibilidad();
    }

    /// <summary>
    /// Desactiva y oculta la parcela, reiniciando su estado por completo.
    /// Se llama al reiniciar el progreso del jugador.
    /// </summary>
    public void Bloquear()
    {
        desbloqueada = false;
        nivel        = 0;
        timerActual  = 0f;
        if (barraProgreso != null) barraProgreso.fillAmount = 0f;
        ActualizarVisibilidad();
    }
}
