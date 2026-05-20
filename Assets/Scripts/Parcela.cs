using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Parcela : MonoBehaviour
{
    [Header("Configuración")]
    public double dineroPerCiclo = 10;
    public float tiempoCiclo = 5f; // segundos

    [Header("UI")]
    public Image barraProgreso; // imagen con fill amount
    public TextMeshProUGUI textDinero;

    private float timerActual = 0f;
    public bool desbloqueada = false;

    void Update()
    {
        if (!desbloqueada) return;

        timerActual += Time.deltaTime;
        
        // Actualiza barra de progreso
        if (barraProgreso != null)
            barraProgreso.fillAmount = timerActual / tiempoCiclo;

        // Cuando completa el ciclo
        if (timerActual >= tiempoCiclo)
        {
            timerActual = 0f;
            GameManager.Instance.AñadirDinero(dineroPerCiclo);
        }
    }

    public void Desbloquear()
    {
        desbloqueada = true;
    }
}