// Gestor principal del juego.
// Almacena el estado global de la partida (dinero, nivel, prestige, parcelas)
// y proporciona métodos de utilidad compartidos por el resto de managers.
// Implementa el patrón Singleton con DontDestroyOnLoad para persistir entre escenas.

using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    /// <summary>Instancia única accesible desde cualquier script.</summary>
    public static GameManager Instance;

    //  Economía del jugador 
    [Header("Dinero")]
    public double dinero = 0;                    // Saldo actual
    public double dineroTotal = 0;               // Dinero acumulado histórico (para ranking)
    public double clickPower = 1;                // Dinero ganado por cada clic manual
    public double prestigeMultiplicador = 1.0;   // Multiplicador permanente otorgado por prestige
    public int    nivel = 1;                     // Nivel del jugador (= parcelas compradas)
    public int    vecesPrestige = 0;             // Número de veces que se ha hecho prestige

    [Tooltip("Texto de la UI que muestra el saldo actual.")]
    public TextMeshProUGUI textDinero;

    [Tooltip("Texto de la UI que muestra la producción por segundo.")]
    public TextMeshProUGUI txtProduccion;

    //  Parcelas 
    [Header("Parcelas")]
    public int     parcelasCompradasTotal = 0;
    public Parcela[] parcelas;                   // Array indexado por número de producto (0-8)

    /// <summary>Producciones base de cada producto en dinero/ciclo (índice = número de producto).</summary>
    public static readonly double[] ProduccionesBase = { 5, 10, 20, 35, 60, 100, 200, 500, 1000 };

    /// <summary>Duración base del ciclo de producción de cada producto en segundos.</summary>
    public static readonly float[]  TiemposBase      = { 2f, 5f, 10f, 15f, 20f, 30f, 60f, 120f, 240f };

    //  Popup offline 
    [Header("Popup Offline")]
    public GameObject      panelOffline;
    public TextMeshProUGUI txtOffline;

    //  Banner de error de red 
    [Header("Error de Red")]
    public GameObject      panelErrorRed;
    public TextMeshProUGUI txtErrorRed;

    /// <summary>Evita mostrar múltiples banners de error simultáneamente.</summary>
    private bool mostrandoErrorRed = false;

    //  Ciclo de vida Unity 

    void Awake()
    {
        // Singleton: solo existe una instancia y sobrevive a los cambios de escena
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Actualizar el texto del saldo en cada frame
        if (textDinero != null)
            textDinero.text = FormatearDinero(dinero);

        // Actualizar el indicador de producción por segundo
        if (txtProduccion != null)
            ActualizarProduccion();
    }

    //  Producción por segundo 

    /// <summary>
    /// Calcula la producción total por segundo sumando el rendimiento de todas
    /// las parcelas desbloqueadas y aplicando el multiplicador de prestige.
    /// </summary>
    void ActualizarProduccion()
    {
        double ps = 0;
        if (parcelas != null)
            foreach (Parcela p in parcelas)
                if (p != null && p.desbloqueada)
                    ps += (p.dineroPerCiclo / p.tiempoCiclo) * prestigeMultiplicador;

        txtProduccion.text = $"+{FormatearDinero(ps)}/s";
    }

    //  Economía 

    /// <summary>
    /// Añade dinero al saldo actual y al contador histórico total,
    /// aplicando el multiplicador de prestige vigente.
    /// </summary>
    /// <param name="cantidad">Cantidad base a añadir antes del multiplicador.</param>
    public void AñadirDinero(double cantidad)
    {
        dinero      += cantidad * prestigeMultiplicador;
        dineroTotal += cantidad * prestigeMultiplicador;
    }

    /// <summary>Fuerza la actualización inmediata del texto de dinero en la UI.</summary>
    public void ActualizarUI()
    {
        if (textDinero != null)
            textDinero.text = FormatearDinero(dinero);
    }

    //  Reset de parcelas 

    /// <summary>
    /// Restaura la producción y el tiempo de ciclo de todas las parcelas a sus
    /// valores base definidos en <see cref="ProduccionesBase"/> y <see cref="TiemposBase"/>.
    /// Se llama tras un prestige o reinicio completo para eliminar los efectos acumulados de las mejoras.
    /// </summary>
    public void ResetearParcelasAValoresBase()
    {
        foreach (Parcela p in parcelas)
        {
            if (p == null) continue;

            // El índice del producto se extrae del identificador "producto_N"
            string numStr = p.idProducto.Replace("producto_", "");
            if (int.TryParse(numStr, out int idx) && idx < ProduccionesBase.Length)
            {
                p.dineroPerCiclo = ProduccionesBase[idx];
                p.tiempoCiclo    = TiemposBase[idx];
            }
        }
    }

    //  Popup de ganancias offline 

    /// <summary>
    /// Muestra un popup informando al jugador del dinero ganado mientras la aplicación estaba cerrada.
    /// Si el panel no existe en escena, lo crea dinámicamente en tiempo de ejecución.
    /// </summary>
    /// <param name="cantidad">Dinero ganado en modo offline.</param>
    public void MostrarPopupOffline(double cantidad)
    {
        if (panelOffline == null) CrearPopupOffline();
        if (panelOffline == null || txtOffline == null) return;

        txtOffline.text = $"¡Bienvenido de vuelta!\n+<b>{FormatearDinero(cantidad)}</b> mientras estabas fuera.";
        panelOffline.SetActive(true);
        StartCoroutine(OcultarPopupOffline());
    }

    /// <summary>
    /// Crea el panel de ganancias offline en tiempo de ejecución cuando no
    /// está asignado en el Inspector (por ejemplo, tras cambio de escena).
    /// </summary>
    void CrearPopupOffline()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject panel = new GameObject("PopupOffline");
        panel.transform.SetParent(canvas.transform, false);
        UnityEngine.UI.Image bg = panel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.75f);
        rt.anchorMax = new Vector2(0.9f, 0.92f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject txtObj = new GameObject("TextOffline");
        txtObj.transform.SetParent(panel.transform, false);
        TMPro.TextMeshProUGUI txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.fontSize  = 22;
        txt.color     = Color.white;
        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = new Vector2(10, 5);
        rtTxt.offsetMax = new Vector2(-10, -5);

        panelOffline = panel;
        txtOffline   = txt;
        panel.SetActive(false);
    }

    System.Collections.IEnumerator OcultarPopupOffline()
    {
        yield return new WaitForSeconds(4f);
        if (panelOffline != null) panelOffline.SetActive(false);
    }

    //  Banner de error de red 

    /// <summary>
    /// Muestra un banner de error durante 3 segundos cuando falla la conexión con Firebase.
    /// El flag mostrandoErrorRed evita que se acumulen múltiples banners si varios guardados fallan a la vez.
    /// </summary>
    /// <param name="mensaje">Texto a mostrar en el banner.</param>
    public void MostrarErrorRed(string mensaje = "Sin conexion. Comprueba tu internet.")
    {
        if (mostrandoErrorRed || panelErrorRed == null) return;
        txtErrorRed.text = mensaje;
        panelErrorRed.SetActive(true);
        StartCoroutine(OcultarErrorRed());
    }

    System.Collections.IEnumerator OcultarErrorRed()
    {
        mostrandoErrorRed = true;
        yield return new WaitForSeconds(3f);
        if (panelErrorRed != null) panelErrorRed.SetActive(false);
        mostrandoErrorRed = false;
    }

    //  Utilidades 

    /// <summary>
    /// Formatea una cantidad de dinero en notación abreviada legible:
    /// valores ≥ 1.000.000 se muestran en M, valores ≥ 1.000 en K.
    /// </summary>
    /// <param name="cantidad">Valor numérico a formatear.</param>
    /// <returns>Cadena formateada, por ejemplo "1.5M", "250.3K" o "999".</returns>
    public string FormatearDinero(double cantidad)
    {
        if (cantidad >= 1_000_000) return $"{cantidad / 1_000_000:F1}M";
        if (cantidad >= 1_000)     return $"{cantidad / 1_000:F1}K";
        return $"{cantidad:F0}";
    }
}
