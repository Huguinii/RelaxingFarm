// Representa una parcela de cultivo o instalación productiva del juego.
// Cada parcela genera dinero automáticamente al completar sus ciclos de producción cuando está desbloqueada.
// La barra de progreso refleja el avance del ciclo actual en tiempo real.

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

    //  Ciclo de vida Unity 
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

    //  Desbloqueo 

    /// <summary>
    /// Activa la parcela para que empiece a producir dinero.
    /// Se llama al comprar la parcela en la tienda o al cargar una partida guardada.
    /// </summary>
    public void Desbloquear()
    {
        desbloqueada = true;
        nivel        = 1;
    }
}
