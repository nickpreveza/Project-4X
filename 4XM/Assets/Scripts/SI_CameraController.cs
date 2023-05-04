using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SI_CameraController : MonoBehaviour
{
    public static SI_CameraController Instance;

    [SerializeField] float scrollSpeed = 0.01f;
    [SerializeField] float zoomSpeed = 0.01f;
    Vector2 touchDeltaPosition;

    public Vector2 orthoSizeBounds;

    [SerializeField] Vector3 editedPosition;

    Camera mainCamera;
    Camera playerCamera;

    Vector2 touchZeroDelta;
    Vector2 touchOneDelta;
    float editedScrollSpeed;
    float prevTouchDeltaMag = 0;
    float touchDeltaMag = 0;
    float deltaMagnitudeDiff = 0;
    [SerializeField] bool outOfBoundsX;
    [SerializeField] bool outOfBoundsY;
    bool movingBack;

    Vector2 internalBoundsX;
    Vector2 internalBoundsY;

    public WorldHex selectedTile;

    [SerializeField] float internalTouchTimer;
    [SerializeField] float timeToRegisterTap;
    [SerializeField] float timeToRegisterHold;

    bool tapValid;

    [SerializeField] float internalMapHeight;
    [SerializeField] float internalMapWidth;

    Vector3 oldPosition;


    [SerializeField] float moveSpeed;
    bool isDraggingCamera = false;
    float rayLenght;
    Vector3 lastMousePosition;
    Vector3 hitPos;
    Vector3 diff;
    Vector3 zoomDir;

    public bool keyboardControls;
    public bool touchControls;
    public bool zoomEnabled;
    public bool dragEnabled;

    Vector3 cameraTargetOffset;

    [SerializeField] LayerMask interactableMask;

    delegate void UpdateFunction();
    UpdateFunction Update_CurrentFunction;// = Update_DetectMode;

    //Threshold of mousemovement to start drag
    int mouseDragThreshold = 1;
    Vector3 lastMouseGroundPlanePosition;

    //hex panning
    [SerializeField] Vector3 cameraOffsetFromPanTarget;
    Vector3 prevCameraPosition;
    Vector3 targetCameraPosition;
    WorldHex hex;
    bool autoMove;
    Vector3 currentVelocity;
    float smoothTime = 0.5f;

    //zoom
   [SerializeField] float minHeight = 2;
   [SerializeField] float maxHeight = 20;

    int autoPanHexIdentifier;
    public bool repeatSelection;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        oldPosition = this.transform.position;
        mainCamera = transform.GetChild(0).GetComponent<Camera>();
        playerCamera = transform.GetChild(0).GetChild(0).GetComponent<Camera>();
        Update_CurrentFunction = Update_DetectModeStart;
    }

    public void UpdateBounds(int numRows, int numColumns)
    {
        float radius = 1;
        float HexHeight = radius * 2;
        float HexWidth = (Mathf.Sqrt(3) / 2) * HexHeight;
        float HexVerticalSpacing = HexHeight * 0.75f;
        float HexHorizontalSpacing = HexWidth;

        float mapHeight = numRows * HexVerticalSpacing;
        float mapWidth = numColumns * HexHorizontalSpacing;

        internalMapHeight = mapHeight;
        internalMapWidth = mapWidth;
    }

    private void Update()
    {
        if (autoMove)
        {
            AutoHexPan();
            CheckifCameraMoved();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || !Application.isFocused)
        {
            CancelUpdateFunction();
            return;
        }

        

        Update_CurrentFunction();

        if (!IsPointerOverUIObject())
        {
            if (keyboardControls)
            {
                KeyboardInput();
            }

            if (zoomEnabled)
            {
                Update_ScrollZoom();
            }

        }


        lastMousePosition = Input.mousePosition;
        lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
        CheckifCameraMoved();
    }

    void CancelUpdateFunction()
    {
        Update_CurrentFunction = Update_DetectModeStart;
        //clean up any UI associated with the mode
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;

    }

    void Update_DetectModeStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
           // Update_CurrentFunction;
            //left mouse button just went down

        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Vector3.Distance(Input.mousePosition, lastMousePosition) > 1)
            {
                Debug.Log("Canceled late click by dragging");
                return;
            }

            Update_Tap();
            
           
        }
        else if (Input.GetMouseButton(0) && (Vector3.Distance(Input.mousePosition, lastMousePosition) > mouseDragThreshold))
        {
            Update_CurrentFunction = Update_CameraDrag;

            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);     
            Update_CurrentFunction();
        }
        else if (Input.GetMouseButton(1))
        {

        }
    }

    void OLDUpdate() //Still has some functionallity that hasn't been moved 
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (mouseRay.direction.y >= 0)
        {
            Debug.LogError("Why is mouse pointing up?");
            return;
        }
        rayLenght = mouseRay.origin.y / mouseRay.direction.y;
        hitPos = mouseRay.origin + (mouseRay.direction * rayLenght);

        //MouseInput();

        if (touchControls)
        {
            //TouchInput();
        }

        if (keyboardControls)
        {
            KeyboardInput();
        }
        if (zoomEnabled)
        {
            //ZoomInput();
            Update_ScrollZoom();
        }

        if (dragEnabled)
        {
            //DragInput(mouseRay);
        }
      


        CheckifCameraMoved();
    }


    public void PanToHex(WorldHex hex)
    {
        autoPanHexIdentifier = hex.hexIdentifier;
        prevCameraPosition = this.transform.position;
        targetCameraPosition = hex.hexData.PositionFromCamera() + cameraOffsetFromPanTarget * (this.transform.position.y / 60);
        targetCameraPosition.y = this.transform.position.y;
        autoMove = true;
    }

    void AutoHexPan()
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, targetCameraPosition, ref currentVelocity, smoothTime);

        if (Vector3.Distance(this.transform.position, targetCameraPosition) < 0.1)
        {
            autoMove = false;
            currentVelocity = Vector3.zero;
            SI_EventManager.Instance.OnAutoPanCompleted(autoPanHexIdentifier);
        }
    }


    void KeyboardInput()
    {
        Vector3 translate = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        this.transform.Translate(translate * moveSpeed * Time.deltaTime, Space.World);
    }

    /*
     void TouchInput()
     {
         if (Input.touchCount > 0)
         {
             if (Input.touches[0].phase == TouchPhase.Began)
             {
                 internalTouchTimer = 0f;
                 tapValid = true;
             }

             internalTouchTimer += 1 * Time.deltaTime;

             if (Input.touchCount < 2 && Input.GetTouch(0).phase == TouchPhase.Moved)
             {
                 tapValid = false;

                 if (outOfBoundsX || outOfBoundsY)
                 {
                     editedScrollSpeed = scrollSpeed * 0.1f;
                 }
                 else
                 {
                     editedScrollSpeed = scrollSpeed;
                 }
                 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
                 transform.Translate(-touchDeltaPosition.x * editedScrollSpeed, -(touchDeltaPosition.y * editedScrollSpeed), 0);
                 editedPosition = transform.position;
                 editedPosition.z = -5;
                 transform.position = editedPosition;
                 if (transform.position.x > internalBoundsX.y || transform.position.x < internalBoundsX.x)
                 {
                     outOfBoundsX = true;
                 }

                 if (transform.position.y > internalBoundsY.y || transform.position.y < internalBoundsY.x)
                 {
                     outOfBoundsY = true;
                 }
                 //editedPosition.x = Mathf.Clamp(transform.position.x, xBounds.x, xBounds.y);
                 // editedPosition.y = Mathf.Clamp(transform.position.y, yBounds.x, yBounds.y);
                 //editedPosition.z = 0;// Mathf.Clamp(transform.position.z, zBounds.x, zBounds.y);
                 //transform.position = editedPosition;
             }

             if (Input.touchCount == 2)
             {
                 tapValid = false;

                 Touch touchZero = Input.GetTouch(0);
                 Touch touchOne = Input.GetTouch(1);

                 touchZeroDelta = touchZero.position - touchZero.deltaPosition;
                 touchOneDelta = touchOne.position - touchOne.deltaPosition;

                 prevTouchDeltaMag = (touchZeroDelta - touchOneDelta).magnitude;
                 touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                 deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                 mainCamera.orthographicSize += deltaMagnitudeDiff * zoomSpeed;
                 mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, orthoSizeBounds.x, orthoSizeBounds.y);
                 playerCamera.orthographicSize = mainCamera.orthographicSize;

             }

             if (Input.touches[0].phase == TouchPhase.Ended && tapValid)
             {
                 Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
                 RaycastHit hit;

                 if (Physics.Raycast(ray, out hit))
                 {
                     if (hit.transform.CompareTag("Tile"))
                     {
                         if (internalTouchTimer > timeToRegisterTap && internalTouchTimer < timeToRegisterHold)
                         {
                             WorldHex newTile = hit.transform.parent.parent.gameObject.GetComponent<WorldHex>();
                             SelectTile(newTile);
                             return;
                         }

                         if (internalTouchTimer >= timeToRegisterHold)
                         {
                             selectedTile = hit.transform.gameObject.GetComponentInParent<WorldHex>();
                             if (selectedTile != null)
                             {
                                 selectedTile.Hold();
                                 internalTouchTimer = 0;
                                 tapValid = false;
                             }
                         }
                     }
                     else
                     {
                         layerAccessor = 0;
                         selectedTile = null;
                     }
                 }
                 else
                 {
                     layerAccessor = 0;
                     selectedTile = null;
                 }
             }

             if (Input.touches[0].phase == TouchPhase.Stationary && tapValid)
             {
                 Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
                 RaycastHit hit;

                 if (Physics.Raycast(ray, out hit))
                 {
                     if (hit.transform.CompareTag("Tile"))
                     {
                         if (internalTouchTimer >= timeToRegisterHold)
                         {
                             selectedTile = hit.transform.gameObject.GetComponentInParent<WorldHex>();
                             if (selectedTile != null)
                             {
                                 selectedTile.Hold();
                                 internalTouchTimer = 0;
                                 tapValid = false;
                             }
                         }
                     }
                 }
             }
         }

         if (Input.touchCount == 0 && !movingBack)
         {
             if (outOfBoundsX || outOfBoundsY)
             {
                 movingBack = true;
                 //StartCoroutine(ReturnToBounds());
             }
         }

         if (Input.touchCount == 0 && movingBack)
         {
             editedPosition.x = Mathf.Clamp(transform.position.x, internalBoundsX.x, internalBoundsX.y);
             editedPosition.y = Mathf.Clamp(transform.position.y, internalBoundsY.x, internalBoundsY.y);
             editedPosition.z = -5;// Mathf.Clamp(transform.position.z, zBounds.x, zBounds.y);

             var step = scrollSpeed * Time.deltaTime;
             transform.position = editedPosition; //= editedPosition;

             if (transform.position == editedPosition)
             {
                 movingBack = false;
                 outOfBoundsY = false;
                 outOfBoundsX = false;
             }

         }
     } */
    void Update_Tap()
    {
        if (IsPointerOverUIObject())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableMask))
        {
            if (hit.transform.CompareTag("Tile"))
            {
                WorldHex newTile = hit.transform.parent.parent.gameObject.GetComponent<WorldHex>();
               
                SelectTile(newTile);
                PanToHex(newTile);
            }
            else
            {
                selectedTile = null;
            }
        }
        else
        {
            selectedTile = null;
        }


        if (outOfBoundsX || outOfBoundsY)
        {
            movingBack = true;
            //StartCoroutine(ReturnToBounds());
        }

        if (movingBack)
        {
            editedPosition.x = Mathf.Clamp(transform.position.x, internalBoundsX.x, internalBoundsX.y);
            editedPosition.y = Mathf.Clamp(transform.position.y, internalBoundsY.x, internalBoundsY.y);
            editedPosition.z = -5;// Mathf.Clamp(transform.position.z, zBounds.x, zBounds.y);

            var step = scrollSpeed * Time.deltaTime;
            transform.position = editedPosition; //= editedPosition;

            if (transform.position == editedPosition)
            {
                movingBack = false;
                outOfBoundsY = false;
                outOfBoundsX = false;
            }

        }
    }


    void Update_CameraDrag()
    {
        //Dragging
        if (Input.GetMouseButtonUp(0) ||  IsPointerOverUIObject())
        {
            CancelUpdateFunction();
            return;
        }

        hitPos = MouseToGroundPlane(Input.mousePosition);
        diff = lastMouseGroundPlanePosition - hitPos;
        this.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);
    }

    void ZoomInput()
    {
        //TODO: Smooth zoom 
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");

        Vector3 currentOrthoSize = new Vector3(mainCamera.orthographicSize, 0,0);
        Vector3 adjustedOrthoSize = new Vector3(mainCamera.orthographicSize - scrollAmount * zoomSpeed, 0, 0);
        currentOrthoSize = Vector3.Lerp(currentOrthoSize, adjustedOrthoSize, Time.deltaTime * 5f);
     

        if (Mathf.Abs(scrollAmount) > 0.1f)
        {
            //mainCamera.orthographicSize -= scrollAmount * zoomSpeed * Time.deltaTime * 5f;
            mainCamera.orthographicSize = currentOrthoSize.x;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, orthoSizeBounds.x, orthoSizeBounds.y);
            playerCamera.orthographicSize = mainCamera.orthographicSize;


            float lowZoom = orthoSizeBounds.x + 3; //dunno don't ask
            float highZoom = orthoSizeBounds.y - 3;

            /*
            if (mainCamera.orthographicSize < lowZoom)
            {
                this.transform.rotation = Quaternion.Euler(
    Mathf.Lerp(45, 50, ((mainCamera.orthographicSize - orthoSizeBounds.x) / (lowZoom - orthoSizeBounds.x))),
    this.transform.rotation.eulerAngles.y,
    this.transform.rotation.eulerAngles.z);
            }

            else if (mainCamera.orthographicSize > highZoom)
            {
                this.transform.rotation = Quaternion.Euler(
    Mathf.Lerp(50, 60, ((mainCamera.orthographicSize - highZoom) / (orthoSizeBounds.y - highZoom))),
    this.transform.rotation.eulerAngles.y,
    this.transform.rotation.eulerAngles.z);
            }
            else
            {
                this.transform.rotation = Quaternion.Euler(
  60,
   this.transform.rotation.eulerAngles.y,
   this.transform.rotation.eulerAngles.z);
            } */
               
        }

    }

    Vector3 MouseToGroundPlane(Vector3 mousePos)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        // What is the point at which the mouse ray intersects Y=0
        if (mouseRay.direction.y >= 0)
        {
            //Debug.LogError("Why is mouse pointing up?");
            return Vector3.zero;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }

    void Update_ScrollZoom()
    {
        // Zoom to scrollwheel
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");

        // Move camera towards hitPos
        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);
        Vector3 dir = hitPos - this.transform.position;

        Vector3 p = this.transform.position;

        // Stop zooming out at a certain distance.
        // TODO: Maybe you should still slide around at 20 zoom?
        if (scrollAmount > 0 || p.y < (maxHeight - 0.1f))
        {
            cameraTargetOffset += dir * scrollAmount;
        }
        Vector3 lastCameraPosition = this.transform.position;
        this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + cameraTargetOffset, Time.deltaTime * 5f);
        cameraTargetOffset -= this.transform.position - lastCameraPosition;


        p = this.transform.position;
        if (p.y < minHeight)
        {
            p.y = minHeight;
        }
        if (p.y > maxHeight)
        {
            p.y = maxHeight;
        }

        this.transform.position = p;

        // Change camera angle
        this.transform.rotation = Quaternion.Euler(
            Mathf.Lerp(60, 60,Camera.main.transform.position.y / maxHeight),
            Camera.main.transform.rotation.eulerAngles.y,
            Camera.main.transform.rotation.eulerAngles.z
        );


    }

    void CheckifCameraMoved()
    {
        if (oldPosition != this.transform.position)
        {
            //Something moved the camera
            oldPosition = this.transform.position;

            SI_EventManager.Instance.OnCameraMoved();
        }
    }

    void SelectTile(WorldHex newTile)
    {
        if (newTile == selectedTile)
        {
            if (repeatSelection)
            {
                repeatSelection = false;
                selectedTile.Deselect();
                selectedTile = null;
                return;
            }
            repeatSelection = true;
            selectedTile.Select(repeatSelection);
            internalTouchTimer = 0;
            tapValid = false;

        }
        else
        {
            repeatSelection = false;
            selectedTile = newTile;
            selectedTile.Select(repeatSelection);
            internalTouchTimer = 0;
            tapValid = false;
        }
    }

    IEnumerator ReturnToBounds()
    {
      
        yield return new WaitForSeconds(0.1f);
        editedPosition.x = Mathf.Clamp(transform.position.x, internalBoundsX.x, internalBoundsX.y);
        editedPosition.y = Mathf.Clamp(transform.position.y, internalBoundsY.x, internalBoundsY.y);
        editedPosition.z = 0;// Mathf.Clamp(transform.position.z, zBounds.x, zBounds.y);
        movingBack = true;
       
    }
#if UNITY_IOS || UNITY_ANDROID

#endif
}
