using com.mobilin.games;
using UnityEngine;

public class GameController : MonoBehaviour
{
    #region Fields
    [Header("---Character & Bike References---")]
    [SerializeField] private GameObject player;         
    [SerializeField] private GameObject bikeRider;
    [SerializeField] private Rigidbody bikeRigidbody;

    [Header("----Camera & UI References---")]
    [SerializeField] private GameObject thirdPersonCamera;
    [SerializeField] private GameObject bikeInteractionIndicator;

    [Header("---Player Position---")]
    [SerializeField] private GameObject playerAfterDismountPosition;
    #endregion

    #region Functions

    #region UnityEvents
    private void OnEnable()
    {
        mvThirdPersonController.OnBikeMountZoneEnter += ShowBikeInteractionIndicator;
        mvThirdPersonController.OnBikeMountZoneExit += HideBikeInteractionIndicator;
    }

    private void OnDisable()
    {
        mvThirdPersonController.OnBikeMountZoneEnter -= ShowBikeInteractionIndicator;
        mvThirdPersonController.OnBikeMountZoneExit -= HideBikeInteractionIndicator;
    }
    #endregion
    #region PrivateMethods
    public void MountBike()
    {
        player.SetActive(false);
        bikeRider.SetActive(true);
        bikeRigidbody.isKinematic = false;
        thirdPersonCamera.SetActive(false);
    }

    private void DismountBike()
    {
        player.SetActive(true);
        bikeRider.SetActive(false);
        bikeRigidbody.isKinematic = true;
        thirdPersonCamera.SetActive(true);
        bikeInteractionIndicator.SetActive(false);

        player.transform.position = playerAfterDismountPosition.transform.position;
    }

    private void ShowBikeInteractionIndicator()
    {
        bikeInteractionIndicator.SetActive(true);
    }

    private void HideBikeInteractionIndicator()
    {
        bikeInteractionIndicator.SetActive(false);
    }
    #endregion

    #endregion
}