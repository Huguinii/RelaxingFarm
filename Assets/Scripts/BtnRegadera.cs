// Gestiona el botón de clic manual del jugador (la regadera).
// Cada vez que el jugador pulsa el botón, se añade dinero según el clickPower actual del GameManager, que puede incrementarse mediante mejoras.

using UnityEngine;

public class BtnRegadera : MonoBehaviour
{
    /// <summary>
    /// Multiplicador base del clic.
    /// Se combina con GameManager.clickPower para calcular el dinero total obtenido por pulsación.
    /// </summary>
    public double dineroPerClick = 1;

    /// <summary>
    /// Añade dinero al jugador al pulsar el botón. 
    /// El valor final es dineroPerClick * clickPower, donde clickPower aumenta con la mejora "Regadera Dorada".
    /// Se conecta al evento OnClick del botón en el Inspector de Unity.
    /// </summary>
    public void OnClick()
    {
        GameManager.Instance.AñadirDinero(dineroPerClick * GameManager.Instance.clickPower);
    }
}
