using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera; // jis camera ki taraf dekhna hai

    void Start()
    {
        // agar camera assign na ho to default main camera le lo
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        // object ko camera ki taraf ghumao
        transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward,
                         targetCamera.transform.rotation * Vector3.up);
    }
}
