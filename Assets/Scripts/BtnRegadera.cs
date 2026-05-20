using UnityEngine;

public class BtnRegadera : MonoBehaviour
{
    public double dineroPerClick = 1;

    public void OnClick()
    {
        GameManager.Instance.AñadirDinero(dineroPerClick);
    }
}