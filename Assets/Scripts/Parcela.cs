using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Parcela : MonoBehaviour
{
    [Header("Configuración")]
    public string idProducto;
    public double dineroPerCiclo = 10;
    public float tiempoCiclo = 5f;
    public int nivel = 0;

    [Header("UI")]
    public Image barraProgreso;
    public TextMeshProUGUI textDinero;

    public float timerActual = 0f;
    public bool desbloqueada = false;

    void Update()
    {
        if (!desbloqueada) return;

        timerActual += Time.deltaTime;

        if (barraProgreso != null)
            barraProgreso.fillAmount = timerActual / tiempoCiclo;

        if (timerActual >= tiempoCiclo)
        {
            timerActual = 0f;
            GameManager.Instance.AñadirDinero(dineroPerCiclo);
        }
    }

    public void Desbloquear()
    {
        desbloqueada = true;
        nivel = 1;
    }
}