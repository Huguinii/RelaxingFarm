// Gestiona la pantalla de perfil del jugador: muestra sus estadísticas, permite editar
// el nombre de usuario y proporciona las acciones de reiniciar progreso y eliminar cuenta.
// La eliminación de cuenta borra todos los datos del usuario en Firebase y redirige al login.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class PerfilManager : MonoBehaviour
{
    /// <summary>Instancia única accesible desde cualquier script.</summary>
    public static PerfilManager Instance;

    [Header("Panel")]
    public GameObject panelPerfil;

    [Header("Textos de estadísticas")]
    public TextMeshProUGUI txtNombre;
    public TextMeshProUGUI txtEmail;
    public TextMeshProUGUI txtDineroTotal;
    public TextMeshProUGUI txtNivel;
    public TextMeshProUGUI txtPrestige;

    [Header("Editar Nombre")]
    public GameObject      panelEditarNombre; // Subpanel con el InputField de edición
    public TMP_InputField  inputNombre;

    //  Ciclo de vida Unity 

    void Awake() { Instance = this; }

    //  Abrir / Cerrar 

    /// <summary>Abre el panel de perfil cerrando cualquier otro panel abierto.</summary>
    public void AbrirPerfil()
    {
        TiendaManager.Instance.CerrarTienda();
        MejorasManager.Instance.CerrarMejoras();
        if (ConfiguracionManager.Instance != null) ConfiguracionManager.Instance.CerrarConfiguracion();
        if (RankingManager.Instance        != null) RankingManager.Instance.CerrarRanking();
        panelPerfil.SetActive(true);
        ActualizarPerfil();
    }

    /// <summary>Cierra el panel de perfil y el de edición de nombre si estuviera abierto.</summary>
    public void CerrarPerfil()
    {
        if (panelEditarNombre != null) panelEditarNombre.SetActive(false);
        panelPerfil.SetActive(false);
    }

    //  Actualizar datos del perfil 

    /// <summary>
    /// Refresca los textos de estadísticas con los valores actuales del GameManager
    /// y los datos de usuario almacenados en PlayerPrefs.
    /// </summary>
    void ActualizarPerfil()
    {
        txtNombre.text     = $"Nombre: {PlayerPrefs.GetString("username", "Jugador")}";
        txtEmail.text      = $"Email: {PlayerPrefs.GetString("email", "")}";
        txtDineroTotal.text = $"Dinero total ganado: {GameManager.Instance.FormatearDinero(GameManager.Instance.dineroTotal)}";
        txtNivel.text      = $"Nivel: {GameManager.Instance.nivel} (parcelas compradas)";
        txtPrestige.text   = $"Veces prestige: {GameManager.Instance.vecesPrestige}";
    }

    //  Editar nombre 

    /// <summary>
    /// Abre el subpanel de edición del nombre, pre-rellenando el campo
    /// con el nombre de usuario actual almacenado en PlayerPrefs.
    /// </summary>
    public void AbrirEditorNombre()
    {
        inputNombre.text = PlayerPrefs.GetString("username", "");
        panelEditarNombre.SetActive(true);
    }

    /// <summary>Cierra el subpanel de edición sin guardar cambios.</summary>
    public void CancelarEdicionNombre()
    {
        panelEditarNombre.SetActive(false);
    }

    /// <summary>
    /// Guarda el nuevo nombre de usuario en PlayerPrefs, lo persiste en
    /// Firebase (/usuarios y /rankings) y actualiza la UI del perfil.
    /// </summary>
    public void GuardarNombre()
    {
        string nuevoNombre = inputNombre.text.Trim();
        if (string.IsNullOrEmpty(nuevoNombre)) return;

        PlayerPrefs.SetString("username", nuevoNombre);
        PlayerPrefs.Save();

        panelEditarNombre.SetActive(false);
        ActualizarPerfil();
        StartCoroutine(GuardarNombreEnFirebase(nuevoNombre));
    }

    /// <summary>
    /// Actualiza el campo "nombre" en /usuarios/{uid} y "nombreUsuario" en /rankings/{uid}
    /// mediante peticiones PATCH para mantener la consistencia entre nodos.
    /// </summary>
    IEnumerator GuardarNombreEnFirebase(string nombre)
    {
        string dbUrl = "https://relaxingfarm-a0ab6-default-rtdb.europe-west1.firebasedatabase.app";
        string auth  = $"?auth={FirebaseManager.Instance.idToken}";

        yield return PatchRequest(
            $"{dbUrl}/usuarios/{FirebaseManager.Instance.uid}.json{auth}",
            $"{{\"nombre\":\"{nombre}\"}}");

        yield return PatchRequest(
            $"{dbUrl}/rankings/{FirebaseManager.Instance.uid}.json{auth}",
            $"{{\"nombreUsuario\":\"{nombre}\"}}");
    }

    /// <summary>Realiza una petición HTTP PATCH con el JSON indicado.</summary>
    IEnumerator PatchRequest(string url, string json)
    {
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest req = new UnityWebRequest(url, "PATCH");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        // El error de red se notifica al usuario mediante el banner del GameManager
    }

    //  Reiniciar progreso 

    /// <summary>
    /// Reinicia completamente el progreso del jugador:
    /// dinero, parcelas, mejoras y estadísticas vuelven a sus valores iniciales.
    /// La partida se guarda inmediatamente después para reflejar el cambio en Firebase.
    /// </summary>
    public void ReiniciarProgreso()
    {
        GameManager.Instance.dinero                 = 0;
        GameManager.Instance.dineroTotal            = 0;
        GameManager.Instance.clickPower             = 1;
        GameManager.Instance.prestigeMultiplicador  = 1;
        GameManager.Instance.nivel                  = 1;
        GameManager.Instance.vecesPrestige          = 0;
        GameManager.Instance.parcelasCompradasTotal = 0;

        if (MejorasManager.Instance != null)
            MejorasManager.Instance.ResetearMejoras();

        // Bloquear, ocultar y resetear todas las parcelas
        foreach (Parcela p in GameManager.Instance.parcelas)
            if (p != null) p.Bloquear();

        // Restaurar valores base de producción (eliminar efectos de mejoras)
        GameManager.Instance.ResetearParcelasAValoresBase();
        GameManager.Instance.parcelas[0].Desbloquear();

        StartCoroutine(SaveManager.Instance.GuardarPartida());
        ActualizarPerfil();
        CerrarPerfil();
    }

    //  Eliminar cuenta 

    /// <summary>Inicia el proceso de eliminación de cuenta.</summary>
    public void EliminarCuenta() { StartCoroutine(EliminarCuentaCoroutine()); }

    /// <summary>
    /// Elimina todos los datos del usuario en Firebase Realtime Database
    /// (usuarios, partidas, rankings, logs) y su cuenta en Firebase Auth.
    /// Limpia PlayerPrefs, destruye los managers persistentes y carga el login.
    /// </summary>
    IEnumerator EliminarCuentaCoroutine()
    {
        // Detener el guardado automático para evitar escrituras durante el borrado
        SaveManager.Instance.CancelInvoke("GuardarPeriodicamenteWrapper");

        string uid   = FirebaseManager.Instance.uid;
        string token = FirebaseManager.Instance.idToken;

        // Limpiar credenciales antes de las peticiones para evitar guardados fantasma
        FirebaseManager.Instance.uid     = "";
        FirebaseManager.Instance.idToken = "";

        string dbUrl = "https://relaxingfarm-a0ab6-default-rtdb.europe-west1.firebasedatabase.app";

        // Borrar todos los nodos de datos del usuario en Realtime Database
        yield return BorrarNodo($"{dbUrl}/usuarios/{uid}.json?auth={token}");
        yield return BorrarNodo($"{dbUrl}/partidas/{uid}.json?auth={token}");
        yield return BorrarNodo($"{dbUrl}/rankings/{uid}.json?auth={token}");
        yield return BorrarNodo($"{dbUrl}/logs/{uid}.json?auth={token}");

        // Eliminar la cuenta de Firebase Authentication mediante la Identity Platform API
        string urlAuth  = $"https://identitytoolkit.googleapis.com/v1/accounts:delete?key=AIzaSyBZ9ARv1tesLNlobGT69AnzXwDYsF6TGII";
        string jsonAuth = $"{{\"idToken\":\"{token}\"}}";
        UnityWebRequest reqAuth = new UnityWebRequest(urlAuth, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonAuth);
        reqAuth.uploadHandler   = new UploadHandlerRaw(body);
        reqAuth.downloadHandler = new DownloadHandlerBuffer();
        reqAuth.SetRequestHeader("Content-Type", "application/json");
        yield return reqAuth.SendWebRequest();

        // Borrar todos los datos locales del dispositivo
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Nulificar las instancias Singleton antes de destruir el GameObject,
        // para que LoginScene pueda recrearlos sin conflictos al cargarse
        GameObject go = GameManager.Instance.gameObject;
        GameManager.Instance   = null;
        FirebaseManager.Instance = null;
        SaveManager.Instance   = null;
        Destroy(go);

        SceneManager.LoadScene("LoginScene");
    }

    //  Helpers 

    /// <summary>Realiza una petición HTTP DELETE al nodo de Firebase indicado.</summary>
    IEnumerator BorrarNodo(string url)
    {
        UnityWebRequest req = UnityWebRequest.Delete(url);
        yield return req.SendWebRequest();
    }

    /// <summary>Fuerza un guardado manual de la partida (disponible para botones de UI).</summary>
    public void GuardarManual() { StartCoroutine(SaveManager.Instance.GuardarPartida()); }
}
