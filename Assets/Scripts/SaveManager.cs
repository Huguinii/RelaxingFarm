// Gestiona la persistencia de la partida mediante Firebase Realtime Database.
// Implementa la carga inicial de datos, el guardado periódico y bajo demanda,
// el cálculo de ganancias offline y la renovación automática del token de autenticación.
// Utiliza la API REST de Firebase con peticiones HTTP asíncronas a través de corrutinas de Unity.

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Globalization;

public class SaveManager : MonoBehaviour
{
    /// <summary>Instancia única accesible desde cualquier script.</summary>
    public static SaveManager Instance;

    // URL base de la base de datos Firebase Realtime Database del proyecto
    private const string DB_URL = "https://relaxingfarm-a0ab6-default-rtdb.europe-west1.firebasedatabase.app";

    // Intervalo de guardado automático en segundos (cada 2 minutos)
    private const float INTERVALO_SYNC = 120f;

    // Límite de horas en modo offline que se tienen en cuenta para el cálculo de ganancias
    private const float MAX_HORAS_OFFLINE = 8f;

    //  Ciclo de vida Unity 

    void Awake()
    {
        // Singleton con persistencia entre escenas
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

    /// <summary>Guarda la partida al cerrar la aplicación si el usuario ha iniciado sesión.</summary>
    void OnApplicationQuit()
    {
        if (FirebaseManager.Instance != null && !string.IsNullOrEmpty(FirebaseManager.Instance.uid))
            StartCoroutine(GuardarPartida());
    }

    /// <summary>Guarda la partida al minimizar o pausar la aplicación.</summary>
    void OnApplicationPause(bool pausado)
    {
        if (pausado && FirebaseManager.Instance != null && !string.IsNullOrEmpty(FirebaseManager.Instance.uid))
            StartCoroutine(GuardarPartida());
    }

    //  Carga inicial 

    /// <summary>
    /// Punto de entrada principal tras el login.
    /// Carga todos los datos del usuario desde Firebase, transiciona a la escena MainGame,
    /// localiza los elementos de UI, aplica las mejoras guardadas,
    /// calcula las ganancias offline e inicia los timers de guardado periódico y renovación de token.
    /// </summary>
    /// <param name="uidParam">UID del usuario autenticado.</param>
    /// <param name="tokenParam">Token de acceso de Firebase Auth.</param>
    public IEnumerator CargarTodo(string uidParam = "", string tokenParam = "")
    {
        // Asignar credenciales al FirebaseManager si se reciben como parámetros
        if (!string.IsNullOrEmpty(uidParam))
        {
            FirebaseManager.Instance.uid     = uidParam;
            FirebaseManager.Instance.idToken = tokenParam;
        }

        yield return StartCoroutine(CargarUsuario());

        // Cargar la escena principal y esperar un frame para que se inicialice
        SceneManager.LoadScene("MainGame");
        yield return null;

        // Buscar y asignar referencias a elementos de UI de la nueva escena,
        // ya que el GameManager (DontDestroyOnLoad) no puede referenciarlos desde el editor
        var textoObj = GameObject.Find("TextDinero");
        if (textoObj != null)
            GameManager.Instance.textDinero = textoObj.GetComponent<TMPro.TextMeshProUGUI>();

        var produccionObj = GameObject.Find("TxtProduccion");
        if (produccionObj != null)
            GameManager.Instance.txtProduccion = produccionObj.GetComponent<TMPro.TextMeshProUGUI>();

        var errorRedObj = GameObject.Find("PanelErrorRed");
        if (errorRedObj != null)
        {
            GameManager.Instance.panelErrorRed = errorRedObj;
            GameManager.Instance.txtErrorRed   = errorRedObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }

        // Ordenar las parcelas en el array por su índice de producto (extraído del idProducto)
        Parcela[] todasParcelas = FindObjectsOfType<Parcela>();
        GameManager.Instance.parcelas = new Parcela[9];
        foreach (Parcela p in todasParcelas)
        {
            string numStr = p.idProducto.Replace("producto_", "");
            if (int.TryParse(numStr, out int indice))
                GameManager.Instance.parcelas[indice] = p;
        }

        yield return StartCoroutine(CargarOCrearPartida());
        yield return StartCoroutine(CargarProductos());
        yield return StartCoroutine(CargarMejoras());

        // Re-aplicar los efectos de las mejoras compradas sobre las parcelas ya cargadas
        if (MejorasManager.Instance != null)
            MejorasManager.Instance.ReaplicarTodasMejoras();

        CalcularDineroOffline();
        GameManager.Instance.ActualizarUI();

        // Iniciar el guardado automático cada INTERVALO_SYNC segundos
        InvokeRepeating("GuardarPeriodicamenteWrapper", INTERVALO_SYNC, INTERVALO_SYNC);

        // Renovar el token de Firebase cada 50 minutos (el token expira a los 60 min)
        InvokeRepeating("RefrescarTokenWrapper", 3000f, 3000f);
    }

    /// <summary>Envoltorio para iniciar la corrutina de guardado desde InvokeRepeating.</summary>
    void GuardarPeriodicamenteWrapper() => StartCoroutine(GuardarPartida());

    /// <summary>Envoltorio para renovar el token desde InvokeRepeating.</summary>
    void RefrescarTokenWrapper() => StartCoroutine(FirebaseManager.Instance.RefrescarToken(null));

    //  Cargar usuario 

    /// <summary>
    /// Descarga los datos globales del usuario (dinero, nivel, prestige…)
    /// desde el nodo /usuarios/{uid} de Firebase y los aplica al GameManager.
    /// También sincroniza el nombre de usuario con PlayerPrefs.
    /// </summary>
    IEnumerator CargarUsuario()
    {
        string url = $"{DB_URL}/usuarios/{FirebaseManager.Instance.uid}.json?auth={FirebaseManager.Instance.idToken}";
        yield return EnviarGet(url, (response, error) =>
        {
            if (error != null || response == "null") return;

            // Se usa CultureInfo.InvariantCulture para evitar errores con el separador
            // decimal en locales que usan coma (como el español)
            GameManager.Instance.dinero                  = double.Parse(ExtraerCampo(response, "dinero"),                  CultureInfo.InvariantCulture);
            GameManager.Instance.dineroTotal             = double.Parse(ExtraerCampo(response, "dineroTotal"),             CultureInfo.InvariantCulture);
            GameManager.Instance.clickPower              = double.Parse(ExtraerCampo(response, "clickPower"),              CultureInfo.InvariantCulture);
            GameManager.Instance.nivel                   = int.Parse(ExtraerCampo(response, "nivel"));
            GameManager.Instance.prestigeMultiplicador  = double.Parse(ExtraerCampo(response, "prestigeMultiplicador"),   CultureInfo.InvariantCulture);
            GameManager.Instance.vecesPrestige           = int.Parse(ExtraerCampo(response, "vecesPrestige"));
            GameManager.Instance.parcelasCompradasTotal  = int.Parse(ExtraerCampo(response, "parcelasCompradasTotal"));

            // Sincronizar el nombre mostrado en perfil con el almacenado en Firebase
            string nombre = ExtraerCampo(response, "nombre");
            if (!string.IsNullOrEmpty(nombre))
                PlayerPrefs.SetString("username", nombre);
            PlayerPrefs.Save();
        });
    }

    //  Cargar o crear partida 

    /// <summary>
    /// Carga el estado de las parcelas y las mejoras desde /partidas/{uid}.
    /// Si no existe ninguna partida guardada, crea una nueva con la parcela inicial.
    /// </summary>
    IEnumerator CargarOCrearPartida()
    {
        string url = $"{DB_URL}/partidas/{FirebaseManager.Instance.uid}.json?auth={FirebaseManager.Instance.idToken}";
        yield return EnviarGet(url, (response, error) =>
        {
            if (error != null || response == "null")
            {
                // Primera vez que el usuario entra: crear partida con la parcela 0 desbloqueada
                StartCoroutine(CrearPartidaNueva());
            }
            else
            {
                // Restaurar el estado de cada parcela desde su bloque JSON
                Parcela[] parcelas = FindObjectsOfType<Parcela>();
                foreach (Parcela parcela in parcelas)
                {
                    string id = parcela.idProducto;
                    if (response.Contains(id))
                    {
                        string bloque = ExtraerBloque(response, id);
                        parcela.desbloqueada = ExtraerCampo(bloque, "desbloqueada") == "true";
                        parcela.nivel        = int.Parse(ExtraerCampo(bloque, "nivel"));
                        parcela.timerActual  = float.Parse(ExtraerCampo(bloque, "timerActual"), CultureInfo.InvariantCulture);

                        // Restaurar la producción guardada (si existe y es válida)
                        string dineroStr = ExtraerCampo(bloque, "dineroPerCiclo");
                        if (!string.IsNullOrEmpty(dineroStr) && dineroStr != "0")
                            parcela.dineroPerCiclo = double.Parse(dineroStr, CultureInfo.InvariantCulture);

                        // El tiempo de ciclo se toma siempre del array base (no se persiste
                        // porque las mejoras se re-aplican sobre el valor base al cargar)
                        float[] tiemposProd = { 2f, 5f, 10f, 15f, 20f, 30f, 60f, 120f, 240f };
                        string numStr = parcela.idProducto.Replace("producto_", "");
                        if (int.TryParse(numStr, out int prodIdx) && prodIdx < tiemposProd.Length)
                            parcela.tiempoCiclo = tiemposProd[prodIdx];

                        // Actualizar la visibilidad del CanvasGroup según el estado cargado.
                        // Es necesario llamarlo explícitamente porque los campos se asignan
                        // de forma directa (sin pasar por Desbloquear()), por lo que el
                        // CanvasGroup no se actualiza automáticamente durante la carga.
                        parcela.ActualizarVisibilidad();
                    }
                }

                // Restaurar los niveles de cada mejora comprada
                if (response.Contains("mejoras") && MejorasManager.Instance != null)
                {
                    string bloqueMejoras = ExtraerBloque(response, "mejoras");
                    for (int i = 0; i < 6; i++)
                    {
                        string idMejora = $"mejora_{i}";
                        if (bloqueMejoras.Contains(idMejora))
                        {
                            string bloque = ExtraerBloque(bloqueMejoras, idMejora);
                            MejorasManager.Instance.nivelesActuales[i] = int.Parse(ExtraerCampo(bloque, "nivelActual"));
                        }
                    }
                }
            }
        });
    }

    /// <summary>
    /// Crea la entrada inicial en Firebase para una partida nueva,
    /// desbloqueando la primera parcela (producto_0).
    /// </summary>
    IEnumerator CrearPartidaNueva()
    {
        string url  = $"{DB_URL}/partidas/{FirebaseManager.Instance.uid}/parcelas/producto_0.json?auth={FirebaseManager.Instance.idToken}";
        string json = "{\"desbloqueada\":true,\"nivel\":1,\"timerActual\":0,\"dineroPerCiclo\":5}";
        yield return EnviarPut(url, json, (response, error) =>
        {
            // Desbloquear la parcela inicial en escena
            Parcela[] parcelas = FindObjectsOfType<Parcela>();
            foreach (Parcela p in parcelas)
                if (p.idProducto == "producto_0") p.Desbloquear();
        });
    }

    // Los métodos CargarProductos y CargarMejoras están reservados para futuras
    // expansiones que requieran datos globales del servidor.
    IEnumerator CargarProductos()
    {
        string url = $"{DB_URL}/productos.json?auth={FirebaseManager.Instance.idToken}";
        yield return EnviarGet(url, (response, error) => { });
    }

    IEnumerator CargarMejoras()
    {
        string url = $"{DB_URL}/mejoras.json?auth={FirebaseManager.Instance.idToken}";
        yield return EnviarGet(url, (response, error) => { });
    }

    //  Guardar partida 

    /// <summary>
    /// Persiste el estado completo de la partida en Firebase:
    /// datos de usuario, estado de cada parcela, ranking global y niveles de mejoras.
    /// Muestra un banner de error si la conexión falla.
    /// </summary>
    public IEnumerator GuardarPartida()
    {
        // 1. Guardar datos globales del jugador en /usuarios/{uid}
        string urlUsuario  = $"{DB_URL}/usuarios/{FirebaseManager.Instance.uid}.json?auth={FirebaseManager.Instance.idToken}";
        string jsonUsuario = $"{{\"dinero\":{GameManager.Instance.dinero.ToString(CultureInfo.InvariantCulture)},"
                           + $"\"dineroTotal\":{GameManager.Instance.dineroTotal.ToString(CultureInfo.InvariantCulture)},"
                           + $"\"nivel\":{GameManager.Instance.nivel},"
                           + $"\"clickPower\":{GameManager.Instance.clickPower.ToString(CultureInfo.InvariantCulture)},"
                           + $"\"prestigeMultiplicador\":{GameManager.Instance.prestigeMultiplicador.ToString(CultureInfo.InvariantCulture)},"
                           + $"\"vecesPrestige\":{GameManager.Instance.vecesPrestige},"
                           + $"\"parcelasCompradasTotal\":{GameManager.Instance.parcelasCompradasTotal},"
                           + $"\"ultimaConexion\":\"{DateTime.UtcNow:o}\"}}";

        yield return EnviarPatch(urlUsuario, jsonUsuario, (res, err) =>
        {
            // Si el primer PATCH falla, hay problema de red: informar al usuario
            if (err != null && GameManager.Instance != null)
                GameManager.Instance.MostrarErrorRed();
        });

        // 2. Guardar el estado individual de cada parcela en /partidas/{uid}/parcelas/
        Parcela[] parcelas = FindObjectsOfType<Parcela>();
        foreach (Parcela parcela in parcelas)
        {
            string urlParcela  = $"{DB_URL}/partidas/{FirebaseManager.Instance.uid}/parcelas/{parcela.idProducto}.json?auth={FirebaseManager.Instance.idToken}";
            string jsonParcela = $"{{\"desbloqueada\":{parcela.desbloqueada.ToString().ToLower()},"
                               + $"\"nivel\":{parcela.nivel},"
                               + $"\"timerActual\":{parcela.timerActual.ToString(CultureInfo.InvariantCulture)},"
                               + $"\"dineroPerCiclo\":{parcela.dineroPerCiclo.ToString(CultureInfo.InvariantCulture)}}}";
            yield return EnviarPatch(urlParcela, jsonParcela, null);
        }

        // 3. Actualizar el ranking global del jugador en /rankings/{uid}
        string urlRanking  = $"{DB_URL}/rankings/{FirebaseManager.Instance.uid}.json?auth={FirebaseManager.Instance.idToken}";
        string jsonRanking = $"{{\"nombreUsuario\":\"{PlayerPrefs.GetString("username")}\","
                           + $"\"dineroTotal\":{GameManager.Instance.dineroTotal.ToString(CultureInfo.InvariantCulture)},"
                           + $"\"nivel\":{GameManager.Instance.nivel}}}";
        yield return EnviarPatch(urlRanking, jsonRanking, null);

        // 4. Guardar timestamp en PlayerPrefs para calcular ganancias offline en la siguiente sesión
        PlayerPrefs.SetString("ultimaConexion", DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();

        // 5. Guardar el nivel de cada mejora comprada en /partidas/{uid}/mejoras/
        if (MejorasManager.Instance != null)
        {
            for (int i = 0; i < 6; i++)
            {
                string urlMejora  = $"{DB_URL}/partidas/{FirebaseManager.Instance.uid}/mejoras/mejora_{i}.json?auth={FirebaseManager.Instance.idToken}";
                string jsonMejora = $"{{\"nivelActual\":{MejorasManager.Instance.nivelesActuales[i]}}}";
                yield return EnviarPatch(urlMejora, jsonMejora, null);
            }
        }
    }

    //  Ganancias offline 

    /// <summary>
    /// Calcula el dinero que el jugador habría generado mientras la app estaba cerrada,
    /// basándose en la producción por segundo de sus parcelas y el tiempo transcurrido desde la última conexión.
    /// El cálculo está limitado a <see cref="MAX_HORAS_OFFLINE"/> para no romper el balance del juego.
    /// </summary>
    void CalcularDineroOffline()
    {
        string ultimaConexionStr = PlayerPrefs.GetString("ultimaConexion", "");
        if (string.IsNullOrEmpty(ultimaConexionStr)) return;

        DateTime ultimaConexion = DateTime.Parse(ultimaConexionStr);
        double horasOffline     = (DateTime.UtcNow - ultimaConexion).TotalHours;
        horasOffline            = Math.Min(horasOffline, MAX_HORAS_OFFLINE);

        if (horasOffline < 0.01f) return;

        // Sumar la producción de todas las parcelas activas
        double produccionPorSegundo = 0;
        Parcela[] parcelas = FindObjectsOfType<Parcela>();
        foreach (Parcela p in parcelas)
            if (p.desbloqueada)
                produccionPorSegundo += p.dineroPerCiclo / p.tiempoCiclo;

        double dineroOffline = produccionPorSegundo * horasOffline * 3600;
        if (dineroOffline > 0)
        {
            GameManager.Instance.AñadirDinero(dineroOffline);
            GameManager.Instance.MostrarPopupOffline(dineroOffline);
        }
    }

    //  Helpers HTTP 

    /// <summary>Realiza una petición HTTP GET a la URL indicada e invoca el callback con la respuesta.</summary>
    IEnumerator EnviarGet(string url, Action<string, string> callback)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            callback(null, req.error);
        else
            callback(req.downloadHandler.text, null);
    }

    /// <summary>Realiza una petición HTTP PUT con el cuerpo JSON indicado.</summary>
    IEnumerator EnviarPut(string url, string json, Action<string, string> callback)
    {
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest req = new UnityWebRequest(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        callback?.Invoke(req.downloadHandler.text, req.result != UnityWebRequest.Result.Success ? req.error : null);
    }

    /// <summary>Realiza una petición HTTP PATCH para actualizar campos concretos en Firebase.</summary>
    IEnumerator EnviarPatch(string url, string json, Action<string, string> callback)
    {
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest req = new UnityWebRequest(url, "PATCH");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        callback?.Invoke(req.downloadHandler.text, req.result != UnityWebRequest.Result.Success ? req.error : null);
    }

    //  Helpers JSON 

    /// <summary>
    /// Extrae el valor de un campo de un JSON compacto.
    /// Soporta valores de tipo cadena (entre comillas) y numérico/booleano.
    /// </summary>
    /// <param name="json">Cadena JSON de origen.</param>
    /// <param name="campo">Nombre del campo a extraer.</param>
    /// <returns>Valor del campo como cadena, o "0" si no se encuentra.</returns>
    string ExtraerCampo(string json, string campo)
    {
        string buscar = $"\"{campo}\":";
        int inicio = json.IndexOf(buscar);
        if (inicio == -1) return "0";
        inicio += buscar.Length;

        bool esString = json[inicio] == '"';
        if (esString)
        {
            inicio++;
            int fin = json.IndexOf("\"", inicio);
            return json.Substring(inicio, fin - inicio);
        }
        else
        {
            // Valor numérico o booleano: termina en coma o llave de cierre
            int fin = json.IndexOfAny(new char[] { ',', '}' }, inicio);
            return json.Substring(inicio, fin - inicio).Trim();
        }
    }

    /// <summary>
    /// Extrae el objeto JSON anidado correspondiente a una clave,
    /// respetando la profundidad de llaves para no cortar objetos internos.
    /// </summary>
    /// <param name="json">JSON de origen.</param>
    /// <param name="clave">Clave del objeto a extraer.</param>
    /// <returns>Subcadena JSON que representa el objeto completo.</returns>
    string ExtraerBloque(string json, string clave)
    {
        string buscar = $"\"{clave}\":{{";
        int inicio    = json.IndexOf(buscar) + buscar.Length - 1;
        int profundidad = 0, fin = inicio;

        for (int i = inicio; i < json.Length; i++)
        {
            if (json[i] == '{') profundidad++;
            if (json[i] == '}') profundidad--;
            if (profundidad == 0) { fin = i; break; }
        }
        return json.Substring(inicio, fin - inicio + 1);
    }
}
