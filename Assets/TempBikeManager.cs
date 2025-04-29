using com.mobilin.games;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TempBikeManager : MonoBehaviour
{
    public mvMotorcycleRider _mvMotorcycleRider;
    public GameObject MountButton;
    public GameObject DisMountButton;

    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.U))
        //{
        //    mount = !mount;

        //    if (mount)
        //    {
        //        _mvMotorcycleRider.EnterInput();
        //        print("A");
        //    }
        //    else if (!mount)
        //    {

        //        _mvMotorcycleRider.ExitInput();
        //        print("B");
        //    }


        //}

    }




    public void MountBike()
    {
        _mvMotorcycleRider.EnterInput();
        disableButtons();
        DisMountButton.SetActive(true);
    }

    public void DisMount()
    {
        _mvMotorcycleRider.ExitInput();
        disableButtons();
        MountButton.SetActive(true);
    }



    private void disableButtons()
    {
        MountButton.SetActive(false);
        DisMountButton.SetActive(false);
    }
}
