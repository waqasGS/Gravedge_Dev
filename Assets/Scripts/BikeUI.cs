using com.mobilin.games;
using UnityEngine;

public class BikeUI : MonoBehaviour
{
    public mvMotorcycleRiderShooter _MvMotorcycleRiderShooter;
    public GameObject MountButton;
    public GameObject DisMountButton;

    public void MountBike()
    {
        _MvMotorcycleRiderShooter.EnterInput();
       OffBothButtons();
        DisMountButton.SetActive(true);
    }

    public void DisMount()
    {
        _MvMotorcycleRiderShooter.ExitInput();
        OffBothButtons();
        MountButton.SetActive(true);
    }

    private void OffBothButtons()
    {
        MountButton.SetActive(false);
        DisMountButton.SetActive(false);
    }




    public void ShowMsg(string msg)
    {
        Debug.Log(msg);
    }
}