using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton para acceder desde cualquier script

    [Header("Dinero")]
    public double dinero = 0;
    public TextMeshProUGUI textDinero;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        textDinero.text = FormatearDinero(dinero);
        ComprobarDesbloqueos();
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

    public void AñadirDinero(double cantidad)
    {
        dinero += cantidad;
    }

    string FormatearDinero(double cantidad)
    {
        if (cantidad >= 1000000) return $"{cantidad / 1000000:F1}M";
        if (cantidad >= 1000) return $"{cantidad / 1000:F1}K";
        return $"{cantidad:F0}";
    }

    [Header("Parcelas")]
    public Parcela[] parcelas; // arrastra las 9 parcelas aquí
    public double[] costeDesbloqueo = { 0, 50, 100, 200, 500, 1000, 2000, 5000, 10000 };

    void Start()
    {
        parcelas[0].Desbloquear(); // la primera gratis
    }

    public void IntentarDesbloquear(int indice)
    {
        if (dinero >= costeDesbloqueo[indice])
        {
            dinero -= costeDesbloqueo[indice];
            parcelas[indice].Desbloquear();
        }
    }
}