using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial1 : MonoBehaviour
{
    public GameObject tutorialHud;
    public GameObject joyStick;
    public GameObject tutorialForJoyStick;
    public GameObject analogCameraMovement;
    public GameObject tutorialCameraMovement;
    public GameObject equipDisplayWindow;
    public GameObject rollButton;
    public GameObject sprintButton;
    public GameObject jumpButton;
    public GameObject unarmedButton;
    public GameObject combatButton;
    public GameObject defenseButton;
    public GameObject slowMoButton;
    public GameObject useButton;
    public GameObject movingArrow;
    public GameObject toActivateUseButton;
    public GameObject healthBar;
    public GameObject healthTutorial;
    public GameObject rightEquip;
    public GameObject leftEquip;
    public GameObject ammoDisplay;
    public GameObject stealthTutorial;
    public GameObject stealthImage1;
    public GameObject stealthImage2;


    public DoorController doorOpening;

    public float delayToStartTutorial;
    public float delayToStartCameraTutorial;
    public float delayInFirstImage;
    public float delayInSecondImage;



    public void Start()
    {
        tutorialHud.SetActive(false);
        joyStick.gameObject.SetActive(false);
        analogCameraMovement.gameObject.SetActive(false);
        equipDisplayWindow.SetActive(false);
        healthBar.SetActive(false);
        healthTutorial.SetActive(false);
        rightEquip.SetActive(false);
        leftEquip.SetActive(false);
        ammoDisplay.SetActive(false);
        rollButton.gameObject.SetActive(false);
        sprintButton.gameObject.SetActive(false);
        jumpButton.gameObject.SetActive(false);
        unarmedButton.gameObject.SetActive(false);
        combatButton.gameObject.SetActive(false);
        defenseButton.gameObject.SetActive(false);
        slowMoButton.gameObject.SetActive(false);
        stealthTutorial.SetActive(false);
        stealthTutorial.transform.DOLocalMoveX(981.8182f, 0.1f);

        Invoke(nameof(CameraMovement), delayToStartTutorial);
    }

    public void JoystickObject()
    {

        tutorialForJoyStick.SetActive(true);
        joyStick.SetActive(true);
    }

    public void ToactivateJoyStickTutorial()
    {
        Invoke(nameof(JoystickObject), delayToStartCameraTutorial);
    }

    public void CameraMovement()
    {
        tutorialHud.SetActive(true);
        tutorialCameraMovement.SetActive(true);
        analogCameraMovement.SetActive(true);
    }

    public void ToshowUseButton()
    {
        useButton.SetActive(true);
    }
    public void ToUnshowUseButton()
    {
        useButton.SetActive(false);
    }
    public void ToShowHealthTutorial()
    {
        Invoke(nameof(ShowingHealthTutorial), delayToStartCameraTutorial);
    }
    public void ShowingHealthTutorial()
    {
        equipDisplayWindow.SetActive(true);
        healthBar.SetActive(true);
        healthTutorial.SetActive(true);
        Invoke(nameof(DeactivatingHealthTutorial), delayToStartCameraTutorial);
    }
    public void DeactivatingHealthTutorial()
    {
        healthTutorial.SetActive(false);
    }
    public void ActivateStealthTutorial()
    {
        //stealthTutorial.GetComponent<CanvasGroup>().alpha = 0.0f;
        stealthImage1.GetComponent<CanvasGroup>().alpha = 0.0f;
        stealthImage2.GetComponent<CanvasGroup>().alpha = 0.0f;
        stealthTutorial.SetActive(true);
        StartCoroutine(StartFadingStealthTutorial());

    }
    IEnumerator StartFadingStealthTutorial()
    {
        //healthBar.SetActive(false);
        stealthTutorial.transform.DOLocalMoveX(581f, 0.5f);
        yield return new WaitForSeconds(0.15f);
        stealthImage1.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        yield return new WaitForSeconds(delayInFirstImage);
        stealthImage1.GetComponent<CanvasGroup>().DOFade(0f, 0.5f);
        stealthImage2.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        yield return new WaitForSeconds(delayInSecondImage);
        stealthImage2.GetComponent<CanvasGroup>().DOFade(0f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        stealthTutorial.transform.DOLocalMoveX(981.8182f, 0.5f).OnComplete(() => { stealthTutorial.SetActive(false);  });



    }
    public void OnClickUseButton()
    {
        movingArrow.SetActive(false);
        useButton.SetActive(false);
        Destroy(toActivateUseButton);
        doorOpening.GetComponent<BoxCollider>().enabled = true;
        doorOpening.SetTarget(doorOpening.openValue, true);
    }
}
