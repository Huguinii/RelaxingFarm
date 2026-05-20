using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string DB_URL = "https://relaxingfarm-a0ab6-default-rtdb.europe-west1.firebasedatabase.app";
    private float timerSync = 0f;
    private const float INTERVALO_SYNC = 300f; // 5 minutos
    private const float MAX_HORAS_OFFLINE = 8f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timerSync += Time.deltaTime;
        if (timerSync >= INTERVALO_SYNC)
        {
            timerSync = 0f;
            StartCoroutine(GuardarPartida());
        }
    }

    void OnApplicationQuit()
    {
        StartCoroutine(GuardarPartida());
    }

    //  CARGA INICIAL 
    public IEnumerator CargarTodo()
    {
        yield return StartCoroutine(CargarUsuario());
        yield return StartCoroutine(CargarOCrearPartida());
        yield return StartCoroutine(CargarProductos());
        yield return StartCoroutine(CargarMejoras());
        CalcularDineroOffline();
        GameManager.Instance.ActualizarUI();
    }

    //  CARGAR USUARIO 
    IEnumerator CargarUsuario()
    {
        string url = $"{DB_URL}/usuarios/{FirebaseManager.Instance.uid}.json?auth={FirebaseManager.Instance.idToken}";
        yield return EnviarGet(url, (response, error) =>
        {
            if (error != null || response == "null") return;
            GameManager.Instance.dinero = double.Parse(ExtraerCampo(response, "dinero"));
            GameManager.Instance.dineroTotal = double.Parse(ExtraerCampo(response, "dineroTotal"));
            GameManager.Instance.clickPower = double.Parse(ExtraerCampo(response, "clickPower"));
            GameManager.Instance.nivel = int.Parse(ExtraerCampo(response, "nivel"));
            GameManager.Instance.prestigeMultiplicador = double.Parse(ExtraerCampo(response, "prestigeMultiplicador"));
        });
    }

    //  CARGAR O CREAR PARTIDA 
    IEnumerator CargarOCrearPartida()
    {
        string url = $"{DB_URL}/partidas/{FirebaseManager.Instance.uid}.json?auth={FirebaseManager.Instance.idToken}";
        yield return EnviarGet(url, (response, error) =>
        {
            if (error != null || response == "null")
            {
                // Primera vez — crear partida nueva
                StartCoroutine(CrearPartidaNueva());
            }
            else
            {
                // Cargar parcelas desde Firebase en los scripts Parcela
                Parcela[] parcelas = FindObjectsOfType<Parcela>();
                foreach (Parcela parcela in parcelas)
                {
                    string id = parcela.idProducto;
                    if (response.Contains(id))
                    {
                        string bloque = ExtraerBloque(response, id);
                        parcela.desbloqueada = ExtraerCampo(bloque, "desbloqueada") == "true";
                        parcela.nivel = int.Parse(ExtraerCampo(bloque, "nivel"));
                        parcela.timerActual = float.Parse(ExtraerCampo(bloque, "timerActual"));
                    }
                }
            }
        });
    }

    //  CREAR PARTIDA NUEVA 
    IEnumerator CrearPartidaNueva()
    {
        string url = $"{DB_URL}/partidas/{FirebaseManager.Instance.uid}/parcelas/producto_0.json?auth={FirebaseManager.Instance.idToken}";
        string json = "{\"desbloqueada\":true,\"nivel\":1,\"timerActual\":0,\"dineroPerCiclo\":5}";
        yield return EnviarPut(url, json, (response, error) =>
        {
            Parcela[] parcelas = FindObjectsOfType<Parcela>();
            foreach (Parcela p in parcelas)
            {
                if (p.idProducto == "producto_0")
                    p.Desbloquear();
            }
        });
    }

    //  CARGAR PRODUCTOS GLOBALES 
    IEnumerator CargarProductos()
    {
        string url = $"{DB_URL}/productos.json?auth={FirebaseManager.Instance.idToken}";
        yield return EnviarGet(url, (response, error) =>
        {
            if (error != null) return;
            // Aquí puedes parsear y guardar el catálogo si lo necesitas en UI
            Debug.Log("Productos cargados: " + response);
        });
    }

    //  CARGAR MEJORAS GLOBALES 
    IEnumerator CargarMejoras()
    {
        string url = $"{DB_URL}/mejoras.json?auth={FirebaseManager.Instance.idToken}";
        yield return EnviarGet(url, (response, error) =>
        {
            if (error != null) return;
            Debug.Log("Mejoras cargadas: " + response);
        });
    }

    //  GUARDAR PARTIDA 
    public IEnumerator GuardarPartida()
    {
        // Guardar usuario
        string urlUsuario = $"{DB_URL}/usuarios/{FirebaseManager.Instance.uid}.json?auth={FirebaseManager.Instance.idToken}";
        string jsonUsuario = $"{{\"dinero\":{GameManager.Instance.dinero}," +
                             $"\"dineroTotal\":{GameManager.Instance.dineroTotal}," +
                             $"\"nivel\":{GameManager.Instance.nivel}," +
                             $"\"clickPower\":{GameManager.Instance.clickPower}," +
                             $"\"prestigeMultiplicador\":{GameManager.Instance.prestigeMultiplicador}," +
                             $"\"ultimaConexion\":\"{DateTime.UtcNow:o}\"}}";
        yield return EnviarPatch(urlUsuario, jsonUsuario, null);

        // Guardar parcelas
        Parcela[] parcelas = FindObjectsOfType<Parcela>();
        foreach (Parcela parcela in parcelas)
        {
            string urlParcela = $"{DB_URL}/partidas/{FirebaseManager.Instance.uid}/parcelas/{parcela.idProducto}.json?auth={FirebaseManager.Instance.idToken}";
            string jsonParcela = $"{{\"desbloqueada\":{parcela.desbloqueada.ToString().ToLower()}," +
                                 $"\"nivel\":{parcela.nivel}," +
                                 $"\"timerActual\":{parcela.timerActual}," +
                                 $"\"dineroPerCiclo\":{parcela.dineroPerCiclo}}}";
            yield return EnviarPatch(urlParcela, jsonParcela, null);
        }

        // Actualizar ranking
        string urlRanking = $"{DB_URL}/rankings/{FirebaseManager.Instance.uid}.json?auth={FirebaseManager.Instance.idToken}";
        string jsonRanking = $"{{\"nombreUsuario\":\"{PlayerPrefs.GetString("username")}\"," +
                              $"\"dineroTotal\":{GameManager.Instance.dineroTotal}," +
                              $"\"nivel\":{GameManager.Instance.nivel}}}";
        yield return EnviarPatch(urlRanking, jsonRanking, null);

        // Caché local
        PlayerPrefs.SetString("dinero", GameManager.Instance.dinero.ToString());
        PlayerPrefs.SetString("ultimaConexion", DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();
    }

    //  DINERO OFFLINE 
    void CalcularDineroOffline()
    {
        string ultimaConexionStr = PlayerPrefs.GetString("ultimaConexion", "");
        if (string.IsNullOrEmpty(ultimaConexionStr)) return;

        DateTime ultimaConexion = DateTime.Parse(ultimaConexionStr);
        double horasOffline = (DateTime.UtcNow - ultimaConexion).TotalHours;
        horasOffline = Math.Min(horasOffline, MAX_HORAS_OFFLINE);

        if (horasOffline < 0.01f) return;

        double produccionPorSegundo = 0;
        Parcela[] parcelas = FindObjectsOfType<Parcela>();
        foreach (Parcela p in parcelas)
        {
            if (p.desbloqueada)
                produccionPorSegundo += p.dineroPerCiclo / p.tiempoCiclo;
        }

        double dineroOffline = produccionPorSegundo * horasOffline * 3600;
        if (dineroOffline > 0)
        {
            GameManager.Instance.AñadirDinero(dineroOffline);
            GameManager.Instance.MostrarPopupOffline(dineroOffline);
        }
    }

    //  HELPERS HTTP 
    IEnumerator EnviarGet(string url, Action<string, string> callback)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            callback(null, req.error);
        else
            callback(req.downloadHandler.text, null);
    }

    IEnumerator EnviarPut(string url, string json, Action<string, string> callback)
    {
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest req = new UnityWebRequest(url, "PUT");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        callback?.Invoke(req.downloadHandler.text, req.result != UnityWebRequest.Result.Success ? req.error : null);
    }

    IEnumerator EnviarPatch(string url, string json, Action<string, string> callback)
    {
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest req = new UnityWebRequest(url, "PATCH");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        callback?.Invoke(req.downloadHandler.text, req.result != UnityWebRequest.Result.Success ? req.error : null);
    }

    //  HELPERS JSON 
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
            int fin = json.IndexOfAny(new char[] { ',', '}' }, inicio);
            return json.Substring(inicio, fin - inicio).Trim();
        }
    }

    string ExtraerBloque(string json, string clave)
    {
        string buscar = $"\"{clave}\":{{";
        int inicio = json.IndexOf(buscar) + buscar.Length - 1;
        int profundidad = 0;
        int fin = inicio;
        for (int i = inicio; i < json.Length; i++)
        {
            if (json[i] == '{') profundidad++;
            if (json[i] == '}') profundidad--;
            if (profundidad == 0) { fin = i; break; }
        }
        return json.Substring(inicio, fin - inicio + 1);
    }
}