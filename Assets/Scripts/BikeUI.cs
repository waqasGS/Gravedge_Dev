using com.mobilin.games;
using UnityEngine;

public class BikeUI : MonoBehaviour
{
    private mvMotorcycleRiderShooter _mvMotorcycleRiderShooter;

    private void Start()
    {
        _mvMotorcycleRiderShooter = GetComponent<mvMotorcycleRiderShooter>();
    }

    public void MountBike()
    {
        _mvMotorcycleRiderShooter.EnterInput();
    }

    public void DisMount()
    {
        _mvMotorcycleRiderShooter.ExitInput();
    }
}