using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMouseDance : MonoBehaviour
{
    public GameObject MouseFXObject;
    public Animator mouseAnimator;
    public RadioInteractable radioInteractable;
    public string message;
    public Color messageColor;

    private bool mouseIsDancing;
    private bool mouseEventActivated;

    private void OnTriggerEnter(Collider other)
    {
        if (mouseEventActivated) return;
        print("trigger enter " + other.name);
        CollisionNotify notifier = other.GetComponent<CollisionNotify>();
        if(notifier != null && notifier.mouseToNotify.ObjectName == "Mylo")
        {
            Destroy(notifier.mouseToNotify.gameObject);
            MouseFXObject.SetActive(true);

            StartCoroutine(ShortMessage());
            mouseEventActivated = true;
        }

    }

    private void Update()
    {
        if (MouseFXObject.activeInHierarchy && radioInteractable.radioOn && !mouseIsDancing)
        {
            mouseAnimator.Play("uniquedance", 0);
            mouseIsDancing = true;
        }
        else if (MouseFXObject.activeInHierarchy && !radioInteractable.radioOn && mouseIsDancing)
        {
            mouseAnimator.Play("enddance", 0);
            mouseIsDancing = false;
        }
    }


    private IEnumerator ShortMessage(float displayTime = 3f) 
    {
        GameManager.current.playerController.UIHandler.DialogueBoxes[0].text = message;
        GameManager.current.playerController.UIHandler.DialogueBoxes[0].color = messageColor;
        yield return new WaitForSeconds(displayTime);
        GameManager.current.playerController.UIHandler.DialogueBoxes[0].text = string.Empty;
    }

    private void OnDestroy()
    {
        if(!string.IsNullOrEmpty(GameManager.current.playerController.UIHandler.DialogueBoxes[0].text)) GameManager.current.playerController.UIHandler.DialogueBoxes[0].text = string.Empty;
    }

}
