using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

public abstract class Interactable : MonoBehaviour
{
    public float overlayOffset;
    public bool isInteractable = true;
    public bool destroyAfterInteraction;
    public float destroyDelay; //if you need to play an animation
    public bool usePlacementOverlay;
    public bool interactableAgain;
    public float delayBetweenInteractions;
    public virtual void Interact()
    {
        isInteractable = false;
        UIManager.Instance.overlayPanel.GetComponent<OverlayPanel>().DisablePrompt();
        if (destroyAfterInteraction)
        {
            StartCoroutine(DestroyWithDelay());
        }
        if (interactableAgain)
        {
            StartCoroutine(InteractableAgain());
        }
    }

    IEnumerator InteractableAgain()
    {
        yield return new WaitForSeconds(delayBetweenInteractions);
        isInteractable = true;
    }

    IEnumerator DestroyWithDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(this.gameObject);
    }
}
