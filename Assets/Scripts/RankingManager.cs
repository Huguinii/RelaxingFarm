// Gestiona la pantalla de ranking global del juego.
// Descarga todos los registros del nodo /rankings de Firebase Realtime Database,
// los ordena por dinero total ganado de mayor a menor y muestra el top 10 de jugadores.

using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

public class RankingManager : MonoBehaviour
{
    /// <summary>Instancia única accesible desde cualquier script.</summary>
    public static RankingManager Instance;

    [Header("Panel")]
    public GameObject panelRanking;

    [Header("UI")]
    public TextMeshProUGUI txtEntradas;  // Texto multilínea con el listado del top 10
    public TextMeshProUGUI txtCargando;  // Texto "Cargando..." visible mientras se descarga

    private const string DB_URL = "https://relaxingfarm-a0ab6-default-rtdb.europe-west1.firebasedatabase.app";

    //  Ciclo de vida Unity 

    void Awake() { Instance = this; }

    //  Abrir / Cerrar 

    /// <summary>
    /// Abre el panel del ranking cerrando los demás paneles e inicia la descarga de datos desde Firebase.
    /// </summary>
    public void AbrirRanking()
    {
        TiendaManager.Instance.CerrarTienda();
        MejorasManager.Instance.CerrarMejoras();
        PerfilManager.Instance.CerrarPerfil();
        if (ConfiguracionManager.Instance != null) ConfiguracionManager.Instance.CerrarConfiguracion();
        panelRanking.SetActive(true);
        StartCoroutine(CargarRanking());
    }

    /// <summary>Cierra el panel del ranking.</summary>
    public void CerrarRanking() { panelRanking.SetActive(false); }

    //  Cargar ranking 

    /// <summary>
    /// Descarga el nodo /rankings completo de Firebase, parsea los registros,
    /// los ordena por dineroTotal descendente y muestra el top 10 en pantalla.
    /// </summary>
    IEnumerator CargarRanking()
    {
        // Mostrar indicador de carga mientras llega la respuesta
        txtCargando.gameObject.SetActive(true);
        txtEntradas.gameObject.SetActive(false);

        string url = $"{DB_URL}/rankings.json?auth={FirebaseManager.Instance.idToken}";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        txtCargando.gameObject.SetActive(false);
        txtEntradas.gameObject.SetActive(true);

        if (req.result != UnityWebRequest.Result.Success)
        {
            txtEntradas.text = "Error al cargar el ranking.";
            yield break;
        }

        string response = req.downloadHandler.text;
        if (response == "null" || string.IsNullOrEmpty(response))
        {
            txtEntradas.text = "Aun no hay jugadores en el ranking.";
            yield break;
        }

        // Parsear, ordenar y mostrar el top 10
        List<RankingEntry> entries = ParsearRanking(response);
        entries.Sort((a, b) => b.dineroTotal.CompareTo(a.dineroTotal));

        StringBuilder sb = new StringBuilder();
        string[] medallas = { "1. ", "2. ", "3. " };
        int max = Mathf.Min(entries.Count, 10);

        for (int i = 0; i < max; i++)
        {
            string puesto = i < 3 ? medallas[i] : $"{i + 1}. ";
            sb.AppendLine($"{puesto}{entries[i].nombreUsuario}   ${FormatearDinero(entries[i].dineroTotal)}   Nv.{entries[i].nivel}");
        }

        txtEntradas.text = sb.ToString();
    }

    //  Parseo de JSON 

    /// <summary>
    /// Parsea el JSON del nodo /rankings de Firebase, que tiene la estructura:
    /// { "uid1": { "nombreUsuario": "X", "dineroTotal": Y, "nivel": Z }, "uid2": {...} }
    /// Extrae cada entrada buscando bloques "{...}" precedidos de ":{".
    /// </summary>
    /// <returns>Lista de entradas de ranking con los datos de cada jugador.</returns>
    List<RankingEntry> ParsearRanking(string json)
    {
        List<RankingEntry> entries = new List<RankingEntry>();
        int i = 0;

        while (i < json.Length)
        {
            // Buscar el inicio del siguiente objeto anidado ("uid":{ )
            int colonBrace = json.IndexOf(":{", i);
            if (colonBrace == -1) break;

            // Extraer el bloque completo respetando profundidad de llaves
            int start = colonBrace + 1;
            int depth = 0, end = start;
            for (int j = start; j < json.Length; j++)
            {
                if (json[j] == '{') depth++;
                if (json[j] == '}') depth--;
                if (depth == 0) { end = j; break; }
            }

            string bloque = json.Substring(start, end - start + 1);

            // Construir la entrada con los campos del bloque
            RankingEntry entry = new RankingEntry();
            entry.nombreUsuario = ExtraerString(bloque, "nombreUsuario");
            double.TryParse(ExtraerValor(bloque, "dineroTotal"), NumberStyles.Any, CultureInfo.InvariantCulture, out entry.dineroTotal);
            int.TryParse(ExtraerValor(bloque, "nivel"), out entry.nivel);

            // Solo añadir entradas con nombre válido (filtra nodos vacíos o corruptos)
            if (!string.IsNullOrEmpty(entry.nombreUsuario))
                entries.Add(entry);

            i = end + 1;
        }
        return entries;
    }

    //  Helpers JSON 

    /// <summary>Extrae el valor de un campo de tipo cadena (entre comillas) de un JSON.</summary>
    string ExtraerString(string json, string campo)
    {
        string buscar = $"\"{campo}\":\"";
        int idx = json.IndexOf(buscar);
        if (idx == -1) return "";
        int inicio = idx + buscar.Length;
        int fin    = json.IndexOf("\"", inicio);
        if (fin == -1) return "";
        return json.Substring(inicio, fin - inicio);
    }

    /// <summary>Extrae el valor de un campo numérico o booleano de un JSON.</summary>
    string ExtraerValor(string json, string campo)
    {
        string buscar = $"\"{campo}\":";
        int idx = json.IndexOf(buscar);
        if (idx == -1) return "0";
        int inicio = idx + buscar.Length;
        int fin    = json.IndexOfAny(new char[] { ',', '}' }, inicio);
        if (fin == -1) return "0";
        return json.Substring(inicio, fin - inicio).Trim();
    }

    //  Utilidades 

    string FormatearDinero(double cantidad)
    {
        if (cantidad >= 1_000_000) return $"{cantidad / 1_000_000:F1}M";
        if (cantidad >= 1_000)     return $"{cantidad / 1_000:F1}K";
        return $"{cantidad:F0}";
    }
}

/// <summary>Modelo de datos para una entrada del ranking global.</summary>
public class RankingEntry
{
    public string nombreUsuario;
    public double dineroTotal;
    public int    nivel;
}
