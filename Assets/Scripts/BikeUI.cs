using System.Collections.Generic;
using com.mobilin.games;
using UnityEngine;
using Invector.vItemManager;
public class BikeUI : MonoBehaviour
{
    public vEquipArea equipArea;
    private mvMotorcycleRiderShooter _mvMotorcycleRiderShooter;
    public List<GameObject> disableInBikeMode = new List<GameObject>();
    private Dictionary<GameObject, bool> originalStates = new Dictionary<GameObject, bool>();

    private void Start()
    {
        _mvMotorcycleRiderShooter = GetComponent<mvMotorcycleRiderShooter>();
    }

    public void MountBike()
    {
        Debug.Log("Mounting Bike");
       equipArea.UnEquipGun_Enter();
        
      Invoke(nameof(InvokeMountBike), 1f);
    }

    private void InvokeMountBike()
    {
        _mvMotorcycleRiderShooter.EnterInput();
        
        // Store the original enabled state of each GameObject
        originalStates.Clear();
        foreach (var item in disableInBikeMode)
        {
            if (item != null)
            {
                originalStates[item] = item.activeSelf;
                item.SetActive(false);
            }
        }
    }

    public void DisMount()
    {
        Debug.Log("Dismounting Bike");
        _mvMotorcycleRiderShooter.ExitInput();
        
        // Only re-enable GameObjects that were originally enabled
        foreach (var item in disableInBikeMode)
        {
            if (item != null && originalStates.ContainsKey(item))
            {
                item.SetActive(originalStates[item]);
            }
        }
        
        
        Invoke(nameof(InvokeDisMount), 1f);
    }



    private void InvokeDisMount()
    {
        equipArea.EquipGun_Exit();
    }
}