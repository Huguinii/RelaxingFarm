// Gestiona la tienda del juego, donde el jugador puede comprar nuevas parcelas de producción.
// Muestra el estado de cada parcela (disponible, asequible o ya comprada)
// y aplica las mejoras activas a las parcelas recién adquiridas.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TiendaManager : MonoBehaviour
{
    /// <summary>Instancia única accesible desde cualquier script.</summary>
    public static TiendaManager Instance;

    [Header("Panel")]
    public GameObject panelTienda;

    [Header("Botones Parcelas")]
    public Button[] botonesParcelas; // Un botón por cada parcela (índice = número de producto).

    // Datos de cada producto (mismo orden que el array de parcelas).
    private string[] nombres     = { "Semilla de Girasol", "Zanahorias", "Tomates", "Maiz",
                                     "Berenjenas", "Girasoles", "Granja de Gallinas",
                                     "Granja de Ovejas", "Granja de Vacas" };
    private double[] costes      = { 0, 50, 100, 200, 500, 1000, 2000, 5000, 10000 };
    private double[] producciones = { 5, 10, 20, 35, 60, 100, 200, 500, 1000 };
    private float[]  tiempos      = { 2, 5, 10, 15, 20, 30, 60, 120, 240 };

    //  Ciclo de vida Unity 

    void Awake() { Instance = this; }

    void Update()
    {
        // Refrescar los botones mientras el panel está visible.
        if (panelTienda.activeSelf)
            ActualizarBotones();
    }

    //  Abrir / Cerrar 

    /// <summary>
    /// Abre el panel de la tienda cerrando previamente cualquier otro panel abierto.
    /// </summary>
    public void AbrirTienda()
    {
        MejorasManager.Instance.CerrarMejoras();
        PerfilManager.Instance.CerrarPerfil();
        if (ConfiguracionManager.Instance != null) ConfiguracionManager.Instance.CerrarConfiguracion();
        if (RankingManager.Instance        != null) RankingManager.Instance.CerrarRanking();
        panelTienda.SetActive(true);
        ActualizarBotones();
    }

    /// <summary>Cierra el panel de la tienda.</summary>
    public void CerrarTienda() { panelTienda.SetActive(false); }

    //  Actualizar botones 

    /// <summary>
    /// Actualiza el texto y el color de cada botón según el estado de la parcela:
    /// verde si ya está comprada, negro si no se puede pagar, dorado si está disponible.
    /// </summary>
    void ActualizarBotones()
    {
        for (int i = 0; i < botonesParcelas.Length; i++)
        {
            Button btn        = botonesParcelas[i];
            bool yaComprada   = GameManager.Instance.parcelas[i].desbloqueada;
            bool puedePagar   = GameManager.Instance.dinero >= costes[i];

            TextMeshProUGUI texto = btn.GetComponentInChildren<TextMeshProUGUI>();
            texto.color = Color.white;

            if (yaComprada)
                texto.text = $"{nombres[i]}\n✓ Comprada";
            else
                texto.text = $"{nombres[i]}\n${FormatearDinero(costes[i])}\n+{producciones[i]}/ciclo";

            // Código de colores: verde = comprada, negro = sin fondos, dorado = disponible
            Image img = btn.GetComponent<Image>();
            if (yaComprada)         img.color = new Color(0.3f, 0.7f, 0.3f);
            else if (!puedePagar)   img.color = new Color(0.2f, 0.2f, 0.2f);
            else                    img.color = new Color(0.8f, 0.6f, 0.2f);

            btn.interactable = !yaComprada;
        }
    }

    //  Comprar parcela 

    /// <summary>
    /// Procesa la compra de una parcela: descuenta el coste, configura los valores de producción,
    /// desbloquea la parcela, aplica las mejoras activas y guarda.
    /// </summary>
    /// <param name="indice">Índice de la parcela a comprar (0-8).</param>
    public void ComprarParcela(int indice)
    {
        if (GameManager.Instance.parcelas[indice].desbloqueada) return;
        if (GameManager.Instance.dinero < costes[indice]) return;

        GameManager.Instance.dinero -= costes[indice];

        // Asignar los valores de producción base del producto seleccionado
        GameManager.Instance.parcelas[indice].dineroPerCiclo = producciones[indice];
        GameManager.Instance.parcelas[indice].tiempoCiclo    = tiempos[indice];
        GameManager.Instance.parcelas[indice].Desbloquear();

        // Aplicar los efectos de las mejoras activas (Riego Rápido, Abono Premium, Silo Mejorado)
        if (MejorasManager.Instance != null)
            MejorasManager.Instance.AplicarMejorasAParcelaNueva(GameManager.Instance.parcelas[indice]);

        GameManager.Instance.parcelasCompradasTotal++;
        GameManager.Instance.nivel = GameManager.Instance.parcelasCompradasTotal;

        StartCoroutine(SaveManager.Instance.GuardarPartida());
        ActualizarBotones();
    }

    //  Utilidades 

    string FormatearDinero(double cantidad)
    {
        if (cantidad >= 1_000_000) return $"{cantidad / 1_000_000:F1}M";
        if (cantidad >= 1_000)     return $"{cantidad / 1_000:F1}K";
        return $"{cantidad:F0}";
    }
}
