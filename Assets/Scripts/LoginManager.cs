using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelLogin;
    public GameObject panelRegister;

    [Header("Login")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public TextMeshProUGUI loginError;

    [Header("Register")]
    public TMP_InputField registerEmail;
    public TMP_InputField registerUsername;
    public TMP_InputField registerPassword;
    public TMP_InputField registerConfirmPassword;
    public TextMeshProUGUI registerError;

    //  NAVEGACION ENTRE PANELES 
    public void MostrarRegister()
    {
        panelLogin.SetActive(false);
        panelRegister.SetActive(true);
    }

    public void MostrarLogin()
    {
        panelRegister.SetActive(false);
        panelLogin.SetActive(true);
    }

    //  LOGIN 
    public void OnClickContinuar()
    {
        string email = loginEmail.text.Trim();
        string password = loginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginError.text = "Rellena todos los campos.";
            return;
        }

        loginError.text = "Iniciando sesion...";
        StartCoroutine(FirebaseManager.Instance.Login(email, password, (exito) =>
        {
            if (exito)
                SceneManager.LoadScene("MainGame");
            else
                loginError.text = "Email o contrasena incorrectos.";
        }));
    }

    //  REGISTER 
    public void OnClickCrear()
    {
        string email = registerEmail.text.Trim();
        string username = registerUsername.text.Trim();
        string password = registerPassword.text;
        string confirm = registerConfirmPassword.text;

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

        registerError.text = "Creando cuenta...";
        StartCoroutine(FirebaseManager.Instance.Registrar(email, password, username, (exito) =>
        {
            if (exito)
                SceneManager.LoadScene("MainGame");
            else
                registerError.text = "Error al crear la cuenta. El email puede estar en uso.";
        }));
    }
}