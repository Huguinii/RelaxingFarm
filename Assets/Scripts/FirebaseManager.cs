// Gestiona la autenticación de usuarios mediante Firebase Authentication (Identity Platform REST API).
// Proporciona métodos para registrar nuevos usuarios, iniciar sesión y renovar el token de acceso.
// Almacena las credenciales activas (UID e idToken) para que el resto de managers puedan credenciales activas (UID e idToken)
// para que el resto de managers puedan realizar peticiones autenticadas a Firebase Realtime Database.

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class FirebaseManager : MonoBehaviour
{
    /// <summary>Instancia única accesible desde cualquier script.</summary>
    public static FirebaseManager Instance;

    // Clave de API pública del proyecto Firebase (Identity Platform)
    private const string API_KEY = "AIzaSyBZ9ARv1tesLNlobGT69AnzXwDYsF6TGII";

    // URL base de Firebase Realtime Database
    private const string DB_URL = "https://relaxingfarm-a0ab6-default-rtdb.europe-west1.firebasedatabase.app";

    /// <summary>Identificador único del usuario autenticado (proporcionado por Firebase Auth).</summary>
    [HideInInspector] public string uid;

    /// <summary>Token de acceso JWT para autorizar peticiones a Firebase (expira en 60 minutos).</summary>
    [HideInInspector] public string idToken;

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
            // Si ya existe una instancia, transferir credenciales si las tiene y destruir el duplicado
            if (!string.IsNullOrEmpty(uid) && string.IsNullOrEmpty(Instance.uid))
            {
                Instance.uid     = uid;
                Instance.idToken = idToken;
            }
            Destroy(gameObject);
        }
    }

    //  Registro 

    /// <summary>
    /// Registra un nuevo usuario en Firebase Authentication y crea su perfil inicial en Realtime Database.
    /// Guarda las credenciales en PlayerPrefs para facilitar futuros auto-logins.
    /// </summary>
    /// <param name="email">Correo electrónico del nuevo usuario.</param>
    /// <param name="password">Contraseña (mínimo 6 caracteres, validado por Firebase).</param>
    /// <param name="nombre">Nombre de usuario visible en el perfil y el ranking.</param>
    /// <param name="callback">Devuelve (éxito, uid, token) al completarse la operación.</param>
    public IEnumerator Registrar(string email, string password, string nombre, Action<bool, string, string> callback)
    {
        string url  = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={API_KEY}";
        string json = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

        string uidLocal   = "";
        string tokenLocal = "";

        yield return EnviarRequest(url, json, (response, error) =>
        {
            if (error != null) { callback(false, "", ""); return; }

            // Extraer UID y token del JSON de respuesta de Firebase Auth
            uidLocal   = ExtraerCampo(response, "localId");
            tokenLocal = ExtraerCampo(response, "idToken");

            Instance.uid     = uidLocal;
            Instance.idToken = tokenLocal;

            // Persistir credenciales localmente para futuras sesiones
            PlayerPrefs.SetString("uid",          uidLocal);
            PlayerPrefs.SetString("idToken",      tokenLocal);
            PlayerPrefs.SetString("refreshToken", ExtraerCampo(response, "refreshToken"));
            PlayerPrefs.SetString("username",     nombre);
            PlayerPrefs.SetString("email",        email);
            PlayerPrefs.Save();

            // Crear el perfil del usuario en Realtime Database
            StartCoroutine(GuardarPerfil(nombre, email, (exito) =>
            {
                callback(exito, uidLocal, tokenLocal);
            }));
        });
    }

    //  Login 

    /// <summary>
    /// Autentica un usuario existente con email y contraseña a través de Firebase Authentication y almacena las credenciales obtenidas.
    /// </summary>
    /// <param name="email">Correo electrónico registrado.</param>
    /// <param name="password">Contraseña del usuario.</param>
    /// <param name="callback">Devuelve (éxito, uid, token) al completarse.</param>
    public IEnumerator Login(string email, string password, Action<bool, string, string> callback)
    {
        string url  = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={API_KEY}";
        string json = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

        string uidLocal   = "";
        string tokenLocal = "";

        yield return EnviarRequest(url, json, (response, error) =>
        {
            if (error != null) { callback(false, "", ""); return; }

            uidLocal   = ExtraerCampo(response, "localId");
            tokenLocal = ExtraerCampo(response, "idToken");

            Instance.uid     = uidLocal;
            Instance.idToken = tokenLocal;

            PlayerPrefs.SetString("uid",          uidLocal);
            PlayerPrefs.SetString("idToken",      tokenLocal);
            PlayerPrefs.SetString("refreshToken", ExtraerCampo(response, "refreshToken"));
            PlayerPrefs.SetString("email",        email);
            PlayerPrefs.Save();

            callback(true, uidLocal, tokenLocal);
        });
    }

    //  Renovar token 

    /// <summary>
    /// Renueva el idToken usando el refreshToken almacenado en PlayerPrefs.
    /// Necesario porque el idToken de Firebase expira a los 60 minutos.
    /// Se llama automáticamente desde SaveManager cada 50 minutos.
    /// </summary>
    /// <param name="callback">Devuelve true si la renovación fue exitosa.</param>
    public IEnumerator RefrescarToken(Action<bool> callback)
    {
        string url          = $"https://securetoken.googleapis.com/v1/token?key={API_KEY}";
        string refreshToken = PlayerPrefs.GetString("refreshToken", "");

        if (string.IsNullOrEmpty(refreshToken)) { callback?.Invoke(false); yield break; }

        string json = $"{{\"grant_type\":\"refresh_token\",\"refresh_token\":\"{refreshToken}\"}}";
        yield return EnviarRequest(url, json, (response, error) =>
        {
            if (error != null) { callback?.Invoke(false); return; }

            string nuevoToken = ExtraerCampo(response, "id_token");
            if (!string.IsNullOrEmpty(nuevoToken))
            {
                idToken = nuevoToken;
                PlayerPrefs.SetString("idToken", idToken);
                PlayerPrefs.Save();
                callback?.Invoke(true);
            }
            else callback?.Invoke(false);
        });
    }

    //  Guardar perfil 

    /// <summary>
    /// Crea el nodo inicial del usuario en /usuarios/{uid} de Realtime Database con todos sus campos en valores por defecto.
    /// Se llama solo al registrarse.
    /// </summary>
    IEnumerator GuardarPerfil(string nombre, string email, Action<bool> callback)
    {
        string url  = $"{DB_URL}/usuarios/{uid}.json?auth={idToken}";
        string json = $"{{\"uid\":\"{uid}\",\"nombre\":\"{nombre}\",\"email\":\"{email}\","
                    + $"\"dinero\":0,\"dineroTotal\":0,\"nivel\":1,"
                    + $"\"parcelasCompradasTotal\":0,\"vecesPrestige\":0,"
                    + $"\"clickPower\":1,\"prestigeMultiplicador\":1,"
                    + $"\"fechaCreacion\":\"{DateTime.UtcNow:o}\"}}";

        yield return EnviarRequest(url, json, (response, error) =>
        {
            callback(error == null);
        }, method: "PUT");
    }

    //  Helper HTTP 

    /// <summary>Envía una petición HTTP con cuerpo JSON a la URL indicada.</summary>
    /// <param name="method">Verbo HTTP a usar (por defecto POST).</param>
    IEnumerator EnviarRequest(string url, string json, Action<string, string> callback, string method = "POST")
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(url, method);
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            callback(null, request.error);
        else
            callback(request.downloadHandler.text, null);
    }

    //  Helper JSON 

    /// <summary>
    /// Extrae el valor de un campo de un JSON de Firebase Auth.
    /// La API de Identity Platform devuelve JSON con espacios tras los dos puntos ("campo": "valor"),
    /// por lo que se omiten los espacios antes de leer el valor.
    /// </summary>
    string ExtraerCampo(string json, string campo)
    {
        string buscar = $"\"{campo}\":";
        int idx = json.IndexOf(buscar);
        if (idx == -1) return "";

        int inicio = idx + buscar.Length;

        // Saltar posibles espacios entre ":" y el valor (JSON pretty-printed)
        while (inicio < json.Length && json[inicio] == ' ') inicio++;

        if (inicio >= json.Length || json[inicio] != '"') return "";
        inicio++;
        int fin = json.IndexOf("\"", inicio);
        if (fin == -1) return "";
        return json.Substring(inicio, fin - inicio);
    }
}
