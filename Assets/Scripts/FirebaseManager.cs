using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    private const string API_KEY = "AIzaSyBZ9ARv1tesLNlobGT69AnzXwDYsF6TGII";
    private const string DB_URL = "https://relaxingfarm-a0ab6-default-rtdb.europe-west1.firebasedatabase.app";

    [HideInInspector] public string uid;
    [HideInInspector] public string idToken;

    void Awake()
    {
        Instance = this;
    }

    // ─── REGISTRO ───────────────────────────────────────────
    public IEnumerator Registrar(string email, string password, string nombre, Action<bool> callback)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={API_KEY}";
        string json = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

        yield return EnviarRequest(url, json, (response, error) =>
        {
            if (error != null) { callback(false); return; }

            uid = ExtraerCampo(response, "localId");
            idToken = ExtraerCampo(response, "idToken");

            // Guardar perfil inicial
            StartCoroutine(GuardarPerfil(nombre, email, callback));
        });
    }

    // ─── LOGIN ───────────────────────────────────────────────
    public IEnumerator Login(string email, string password, Action<bool> callback)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={API_KEY}";
        string json = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

        yield return EnviarRequest(url, json, (response, error) =>
        {
            if (error != null) { callback(false); return; }

            uid = ExtraerCampo(response, "localId");
            idToken = ExtraerCampo(response, "idToken");
            callback(true);
        });
    }

    // ─── GUARDAR PERFIL ──────────────────────────────────────
    IEnumerator GuardarPerfil(string nombre, string email, Action<bool> callback)
    {
        string url = $"{DB_URL}/usuarios/{uid}.json?auth={idToken}";
        string json = $"{{\"nombre\":\"{nombre}\",\"email\":\"{email}\",\"dinero\":0,\"dineroTotal\":0,\"nivel\":1,\"clickPower\":1,\"fechaCreacion\":\"{DateTime.UtcNow:o}\"}}";

        yield return EnviarRequest(url, json, (response, error) =>
        {
            callback(error == null);
        }, method: "PUT");
    }

    // ─── HELPER: ENVIAR REQUEST ──────────────────────────────
    IEnumerator EnviarRequest(string url, string json, Action<string, string> callback, string method = "POST")
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(url, method);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            callback(null, request.error);
        else
            callback(request.downloadHandler.text, null);
    }

    // ─── HELPER: EXTRAER CAMPO JSON ──────────────────────────
    string ExtraerCampo(string json, string campo)
    {
        string buscar = $"\"{campo}\":\"";
        int inicio = json.IndexOf(buscar) + buscar.Length;
        int fin = json.IndexOf("\"", inicio);
        return json.Substring(inicio, fin - inicio);
    }
}