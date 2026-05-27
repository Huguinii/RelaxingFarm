// Gestiona el sistema de mejoras del juego. Cada mejora tiene un coste escalado por nivel y
// un efecto acumulativo sobre la producción o el comportamiento del juego.
// Incluye la mejora especial de Prestige, que reinicia la partida a cambio de un multiplicador permanente de ganancias.

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MejorasManager : MonoBehaviour
{
    /// <summary>Instancia única accesible desde cualquier script.</summary>
    public static MejorasManager Instance;

    [Header("Panel")]
    public GameObject panelMejoras;

    [Header("Botones Mejoras")]
    public Button[] botonesMejoras; // Array de 6 botones, uno por mejora

    //  Datos de cada mejora 

    private string[] nombres = {
        "Riego Rapido", "Abono Premium", "Regadera Dorada",
        "Empleado Basico", "Silo Mejorado", "Cosecha Legendaria"
    };

    private string[] descripciones = {
        "Reduce tiempo de ciclo x0.8", "Aumenta produccion x1.5",
        "Duplica el click power",      "Automatiza clics x0.7",
        "Duplica produccion global x2","PRESTIGE: Reinicia con x1.5 permanente"
    };

    // Coste base de cada mejora en su nivel 0
    private double[] costesBase    = { 500, 1000, 2000, 5000, 10000, 100000 };

    // Factor de escalado del coste por nivel (coste = base * factor^nivelActual)
    private double[] costeEscalado = { 2.0, 2.5, 3.0, 2.0, 3.0, 2.0 };

    // Nivel máximo que puede alcanzar cada mejora
    private int[] nivelesMaximos   = { 5, 5, 3, 3, 3, 10 };

    /// <summary>Nivel actual de cada mejora (público para persistirse en Firebase).</summary>
    public int[] nivelesActuales = { 0, 0, 0, 0, 0, 0 };

    /// <summary>Referencia a la corrutina de auto-click para poder detenerla.</summary>
    private Coroutine autoClickCoroutine;

    //  Ciclo de vida Unity 

    void Awake() { Instance = this; }

    void Update()
    {
        if (panelMejoras.activeSelf)
            ActualizarBotones();
    }

    //  Abrir / Cerrar 

    /// <summary>Abre el panel de mejoras cerrando cualquier otro panel abierto.</summary>
    public void AbrirMejoras()
    {
        TiendaManager.Instance.CerrarTienda();
        PerfilManager.Instance.CerrarPerfil();
        if (ConfiguracionManager.Instance != null) ConfiguracionManager.Instance.CerrarConfiguracion();
        if (RankingManager.Instance        != null) RankingManager.Instance.CerrarRanking();
        panelMejoras.SetActive(true);
        ActualizarBotones();
    }

    /// <summary>Cierra el panel de mejoras.</summary>
    public void CerrarMejoras() { panelMejoras.SetActive(false); }

    //  Actualizar botones 

    /// <summary>
    /// Refresca el texto y el color de cada botón de mejora según el nivel actual,
    /// el nivel máximo y si el jugador tiene fondos suficientes para comprarla.
    /// </summary>
    void ActualizarBotones()
    {
        for (int i = 0; i < botonesMejoras.Length; i++)
        {
            Button btn        = botonesMejoras[i];
            int    nivelActual = nivelesActuales[i];
            int    nivelMax    = nivelesMaximos[i];
            double coste       = ObtenerCoste(i);
            bool   maxLevel    = nivelActual >= nivelMax;
            bool   puedePagar  = GameManager.Instance.dinero >= coste;

            TextMeshProUGUI texto = btn.GetComponentInChildren<TextMeshProUGUI>();
            texto.color = Color.white;

            if (maxLevel)
                texto.text = $"{nombres[i]}\nNivel MAX ({nivelActual}/{nivelMax})\n{descripciones[i]}";
            else
                texto.text = $"{nombres[i]}\nNivel {nivelActual}/{nivelMax}\n${FormatearDinero(coste)}\n{descripciones[i]}";

            // Verde = nivel máximo, negro = sin fondos, dorado = disponible para comprar
            Image img = btn.GetComponent<Image>();
            if (maxLevel)           img.color = new Color(0.3f, 0.7f, 0.3f);
            else if (!puedePagar)   img.color = new Color(0.2f, 0.2f, 0.2f);
            else                    img.color = new Color(0.8f, 0.6f, 0.2f);

            btn.interactable = !maxLevel;
        }
    }

    //  Comprar mejora 

    /// <summary>
    /// Procesa la compra de un nivel de mejora: verifica nivel máximo y fondos,
    /// descuenta el coste, incrementa el nivel y aplica el efecto inmediatamente.
    /// </summary>
    /// <param name="indice">Índice de la mejora a comprar (0-5).</param>
    public void ComprarMejora(int indice)
    {
        if (nivelesActuales[indice] >= nivelesMaximos[indice]) return;
        double coste = ObtenerCoste(indice);
        if (GameManager.Instance.dinero < coste) return;

        GameManager.Instance.dinero -= coste;
        nivelesActuales[indice]++;

        AplicarMejora(indice);
        StartCoroutine(SaveManager.Instance.GuardarPartida());
        ActualizarBotones();
    }

    //  Aplicar efectos 

    /// <summary>
    /// Aplica el efecto de la mejora indicada sobre las parcelas o el estado del juego.
    /// Cada mejora tiene un efecto acumulativo que se apila con los niveles anteriores.
    /// </summary>
    void AplicarMejora(int indice)
    {
        switch (indice)
        {
            case 0: // Riego Rápido: reduce el tiempo de ciclo un 20% en todas las parcelas activas
                foreach (Parcela p in GameManager.Instance.parcelas)
                    if (p != null && p.desbloqueada) p.tiempoCiclo *= 0.8f;
                break;

            case 1: // Abono Premium: incrementa la producción por ciclo un 50%
                foreach (Parcela p in GameManager.Instance.parcelas)
                    if (p != null && p.desbloqueada) p.dineroPerCiclo *= 1.5;
                break;

            case 2: // Regadera Dorada: duplica el dinero obtenido por clic manual
                GameManager.Instance.clickPower *= 2;
                break;

            case 3: // Empleado Básico: inicia el auto-click automático (o lo acelera)
                IniciarAutoClick();
                break;

            case 4: // Silo Mejorado: duplica la producción global de todas las parcelas
                foreach (Parcela p in GameManager.Instance.parcelas)
                    if (p != null && p.desbloqueada) p.dineroPerCiclo *= 2;
                break;

            case 5: // Cosecha Legendaria: activa el PRESTIGE (reinicio con multiplicador)
                AplicarPrestige();
                break;
        }
    }

    //  Prestige 

    /// <summary>
    /// Aplica el mecanismo de Prestige: incrementa el multiplicador permanente de ganancias en x1.5,
    /// reinicia el dinero, las parcelas y las mejoras (excepto el nivel de Cosecha Legendaria)
    /// y desbloquea la primera parcela de nuevo.
    /// </summary>
    void AplicarPrestige()
    {
        // Incrementar el multiplicador permanente antes de reiniciar
        GameManager.Instance.prestigeMultiplicador *= 1.5;
        GameManager.Instance.dinero                 = 0;
        GameManager.Instance.clickPower             = 1;
        GameManager.Instance.vecesPrestige++;
        GameManager.Instance.parcelasCompradasTotal = 0;

        // Conservar el nivel de Cosecha Legendaria (mejora de prestige) al reiniciar
        nivelesActuales = new int[] { 0, 0, 0, 0, 0, nivelesActuales[5] };

        // Bloquear todas las parcelas y resetear su estado
        foreach (Parcela p in GameManager.Instance.parcelas)
        {
            if (p != null)
            {
                p.desbloqueada = false;
                p.nivel        = 0;
                p.timerActual  = 0;
            }
        }

        // Restaurar los valores base de producción antes de desbloquear la primera parcela
        GameManager.Instance.ResetearParcelasAValoresBase();
        if (GameManager.Instance.parcelas[0] != null)
            GameManager.Instance.parcelas[0].Desbloquear();

        // Detener el auto-click ya que el Empleado Básico se resetea con el prestige
        if (autoClickCoroutine != null) { StopCoroutine(autoClickCoroutine); autoClickCoroutine = null; }

        StartCoroutine(SaveManager.Instance.GuardarPartida());
        CerrarMejoras();
    }

    //  Auto-click (Empleado Básico) 

    /// <summary>
    /// Inicia o reinicia la corrutina de auto-click. Si ya había una activa,
    /// la detiene primero para que el nuevo nivel surta efecto inmediatamente.
    /// </summary>
    void IniciarAutoClick()
    {
        if (autoClickCoroutine != null) StopCoroutine(autoClickCoroutine);
        autoClickCoroutine = StartCoroutine(AutoClickCoroutine());
    }

    /// <summary>
    /// Corrutina que simula un clic automático a intervalos decrecientes según el nivel
    /// del Empleado Básico: nivel 1 = 3s, nivel 2 = 2s, nivel 3 = 1s.
    /// </summary>
    System.Collections.IEnumerator AutoClickCoroutine()
    {
        float[] intervalos = { 3f, 2f, 1f };
        while (true)
        {
            // El índice del intervalo depende del nivel actual de la mejora
            int idx = Mathf.Clamp(nivelesActuales[3] - 1, 0, intervalos.Length - 1);
            yield return new WaitForSeconds(intervalos[idx]);
            if (GameManager.Instance != null)
                GameManager.Instance.AñadirDinero(GameManager.Instance.clickPower);
        }
    }

    //  Re-aplicar mejoras al cargar 

    /// <summary>
    /// Re-aplica el efecto acumulado de todas las mejoras sobre las parcelas
    /// /// después de cargar la partida desde Firebase.
    /// Necesario porque los efectos no se almacenan en los valores base, sino que se calculan sobre ellos.
    /// </summary>
    public void ReaplicarTodasMejoras()
    {
        Parcela[] parcelas = GameManager.Instance.parcelas;

        for (int n = 0; n < nivelesActuales[0]; n++)
            foreach (Parcela p in parcelas)
                if (p != null && p.desbloqueada) p.tiempoCiclo *= 0.8f;

        for (int n = 0; n < nivelesActuales[1]; n++)
            foreach (Parcela p in parcelas)
                if (p != null && p.desbloqueada) p.dineroPerCiclo *= 1.5;

        for (int n = 0; n < nivelesActuales[2]; n++)
            GameManager.Instance.clickPower *= 2;

        if (nivelesActuales[3] > 0) IniciarAutoClick();

        for (int n = 0; n < nivelesActuales[4]; n++)
            foreach (Parcela p in parcelas)
                if (p != null && p.desbloqueada) p.dineroPerCiclo *= 2;
    }

    /// <summary>
    /// Aplica las mejoras activas (Riego, Abono y Silo) sobre una parcela recién comprada,
    /// para que nazca con los multiplicadores correctos.
    /// </summary>
    /// <param name="p">Parcela recién desbloqueada.</param>
    public void AplicarMejorasAParcelaNueva(Parcela p)
    {
        for (int n = 0; n < nivelesActuales[0]; n++) p.tiempoCiclo    *= 0.8f;
        for (int n = 0; n < nivelesActuales[1]; n++) p.dineroPerCiclo *= 1.5;
        for (int n = 0; n < nivelesActuales[4]; n++) p.dineroPerCiclo *= 2;
    }

    //  Reset completo 

    /// <summary>
    /// Reinicia todos los niveles de mejora a cero y detiene el auto-click.
    /// Se llama desde PerfilManager al reiniciar el progreso completo.
    /// </summary>
    public void ResetearMejoras()
    {
        nivelesActuales = new int[] { 0, 0, 0, 0, 0, 0 };
        if (autoClickCoroutine != null)
        {
            StopCoroutine(autoClickCoroutine);
            autoClickCoroutine = null;
        }
    }

    //  Utilidades 

    /// <summary>
    /// Calcula el coste del siguiente nivel de una mejora usando la fórmula:
    /// coste = costeBase * factorEscalado ^ nivelActual.
    /// </summary>
    double ObtenerCoste(int indice)
        => costesBase[indice] * Math.Pow(costeEscalado[indice], nivelesActuales[indice]);

    string FormatearDinero(double cantidad)
    {
        if (cantidad >= 1_000_000) return $"{cantidad / 1_000_000:F1}M";
        if (cantidad >= 1_000)     return $"{cantidad / 1_000:F1}K";
        return $"{cantidad:F0}";
    }
}
