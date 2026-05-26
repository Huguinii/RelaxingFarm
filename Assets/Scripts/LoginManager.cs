// Gestiona la interfaz de inicio de sesión y registro de nuevos usuarios.
// Controla la validación de campos, la comunicación con FirebaseManager y
// la transición a la escena principal una vez autenticado el usuario.
// Muestra un panel de carga durante las operaciones asíncronas para evitar
// que el usuario interactúe mientras se procesa la petición.

using UnityEngine;
using TMPro;

public class LoginManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelLogin;       // Panel de inicio de sesión
    public GameObject panelRegister;    // Panel de registro de nueva cuenta
    public GameObject panelCargando;    // Overlay de carga (bloquea la UI durante peticiones)

    [Header("Login")]
    public TMP_InputField      loginEmail;
    public TMP_InputField      loginPassword;
    public TextMeshProUGUI     loginError;      // Texto para mostrar mensajes de error al usuario

    [Header("Register")]
    public TMP_InputField      registerEmail;
    public TMP_InputField      registerUsername;
    public TMP_InputField      registerPassword;
    public TMP_InputField      registerConfirmPassword;
    public TextMeshProUGUI     registerError;

    //  Navegación entre paneles 

    /// <summary>Muestra el panel de registro y oculta el de login.</summary>
    public void MostrarRegister()
    {
        panelLogin.SetActive(false);
        panelRegister.SetActive(true);
    }

    /// <summary>Muestra el panel de login y oculta el de registro.</summary>
    public void MostrarLogin()
    {
        panelRegister.SetActive(false);
        panelLogin.SetActive(true);
    }

    //  Login 

    /// <summary>
    /// Valida los campos del formulario de login y lanza la autenticación contra Firebase.
    /// Si el login es correcto inicia la carga de la partida, si falla muestra el mensaje de error correspondiente.
    /// </summary>
    public void OnClickContinuar()
    {
        string email    = loginEmail.text.Trim();
        string password = loginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginError.text = "Rellena todos los campos.";
            return;
        }

        loginError.text = "";
        MostrarCargando();

        StartCoroutine(FirebaseManager.Instance.Login(email, password, (exito, uid, token) =>
        {
            if (exito)
                // Cargar todos los datos del jugador y transicionar a MainGame
                StartCoroutine(SaveManager.Instance.CargarTodo(uid, token));
            else
            {
                OcultarCargando();
                loginError.text = "Email o contrasena incorrectos.";
            }
        }));
    }

    //  Registro 

    /// <summary>
    /// Valida los campos del formulario de registro (campos vacíos, coincidencia de contraseñas, longitud mínima)
    /// y lanza el proceso de creación de cuenta en Firebase si todo es correcto.
    /// </summary>
    public void OnClickCrear()
    {
        string email    = registerEmail.text.Trim();
        string username = registerUsername.text.Trim();
        string password = registerPassword.text;
        string confirm  = registerConfirmPassword.text;

        // Validaciones del lado del cliente
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
        {
            registerError.text = "Rellena todos los campos.";
            return;
        }

        if (password != confirm)
        {
            registerError.text = "Las contrasenas no coinciden.";
            return;
        }

        if (password.Length < 6)
        {
            registerError.text = "La contrasena debe tener al menos 6 caracteres.";
            return;
        }

        registerError.text = "";
        MostrarCargando();

        StartCoroutine(FirebaseManager.Instance.Registrar(email, password, username, (exito, uid, token) =>
        {
            if (exito)
                StartCoroutine(SaveManager.Instance.CargarTodo(uid, token));
            else
            {
                OcultarCargando();
                registerError.text = "Error al crear la cuenta. El email puede estar en uso.";
            }
        }));
    }

    //  Panel de carga 

    /// <summary>
    /// Muestra el panel de carga que bloquea la UI durante operaciones asíncronas.
    /// El panel se oculta automáticamente en caso de error o al cambiar de escena.
    /// </summary>
    void MostrarCargando()
    {
        if (panelCargando != null) panelCargando.SetActive(true);
    }

    /// <summary>Oculta el panel de carga, restaurando la interactividad de la UI.</summary>
    void OcultarCargando()
    {
        if (panelCargando != null) panelCargando.SetActive(false);
    }
}
