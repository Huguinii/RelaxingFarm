using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Dinero")]
    public double dinero = 0;
    public double dineroTotal = 0;
    public double clickPower = 1;
    public double prestigeMultiplicador = 1.0;
    public int nivel = 1;
    public TextMeshProUGUI textDinero;

    [Header("Parcelas")]
    public Parcela[] parcelas;
    public double[] costeDesbloqueo = { 0, 50, 100, 200, 500, 1000, 2000, 5000, 10000 };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(SaveManager.Instance.CargarTodo());
    }

    void Update()
    {
        textDinero.text = FormatearDinero(dinero);
        ComprobarDesbloqueos();
    }

    public void AñadirDinero(double cantidad)
    {
        dinero += cantidad * prestigeMultiplicador;
        dineroTotal += cantidad * prestigeMultiplicador;
    }

    public void ActualizarUI()
    {
        textDinero.text = FormatearDinero(dinero);
    }

    public void MostrarPopupOffline(double cantidad)
    {
        Debug.Log($"Ganaste {cantidad} mientras estabas fuera");
    }

    void ComprobarDesbloqueos()
    {
        for (int i = 0; i < parcelas.Length; i++)
        {
            if (!parcelas[i].desbloqueada && dinero >= costeDesbloqueo[i])
            {
                parcelas[i].Desbloquear();
            }
        }
    }

    string FormatearDinero(double cantidad)
    {
        if (cantidad >= 1000000) return $"{cantidad / 1000000:F1}M";
        if (cantidad >= 1000) return $"{cantidad / 1000:F1}K";
        return $"{cantidad:F0}";
    }
}