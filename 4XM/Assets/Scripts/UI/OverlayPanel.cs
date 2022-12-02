using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayPanel : UIPanel
{
    public bool showPrompt;
    Vector3 locationPosition;
    GameObject interactTarget;

    [SerializeField] RectTransform interactOverlayPrompt;

    [SerializeField] GameObject normalPromt;
    [SerializeField] GameObject placementPromt;

    [SerializeField] float interactOverlayYOffset;
    [SerializeField] RectTransform canvasRect;
    [SerializeField] bool overlayEnabled;
    bool placementOverlay;

    [Header("Tooltip")]
    public Tooltip tooltip;
   
    private void Awake()
    {
        //DisablePrompt();
        //tooltip.gameObject.SetActive(false);
    }
    public override void Activate()
    {
        base.Activate();
        //DisablePrompt();
    }

    public void EnableOverlayPrompt(GameObject _target, float _offset, bool _placementOverlay)
    {
        if (overlayEnabled)
        {
            if (_target == interactTarget)
            {
                return;
            }
            DisablePrompt();
        }
        placementOverlay = _placementOverlay;
        interactTarget = _target;
        if (!placementOverlay)
        {
            interactOverlayYOffset = _offset;
        }
        else
        {
            interactOverlayYOffset = 0;
        }
       
        showPrompt = true;
        interactOverlayPrompt.gameObject.SetActive(true);
        OpenPromt();
    }

    

    public void OpenPromt() //adaptthisformore
    {
        ClosePrompts();
        if (placementOverlay)
        {
            placementPromt.SetActive(true);
        }
        else
        {
            normalPromt.SetActive(true);
        }
    }

    public void ClosePrompts()
    {
        normalPromt.SetActive(false);
        placementPromt.SetActive(false);

    }

    public void DisablePrompt()
    {
        
        showPrompt = false;
        //interactOverlayPrompt.gameObject.SetActive(false);
        //ClosePrompts();
    }

    public override void Disable()
    {
        base.Disable();
    }

    public override void Setup()
    {
        base.Setup();
    }

    private void Update()
    {
        if (showPrompt)
        {
            if (interactTarget == null)
            {
                DisablePrompt();
                return;
            }
            float offsetPosY = interactTarget.transform.position.y + interactOverlayYOffset;

            // Final position of marker above GO in world space
            Vector3 offsetPos = new Vector3(interactTarget.transform.position.x, offsetPosY, interactTarget.transform.position.z);

            // Calculate *screen* position (note, not a canvas/recttransform position)
            Vector2 canvasPos;
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

            // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out canvasPos);

            // Set
            interactOverlayPrompt.localPosition = canvasPos;
        }
    }

}
