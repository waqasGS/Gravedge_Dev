using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPicking : MonoBehaviour
{
    public void AfterPickUP()
    {
        Debug.Log("i picked you");
    }
    public void DestoringItself()
    {
        Destroy(this.gameObject);
    }
}
