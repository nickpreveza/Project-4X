using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SignedInitiative;

public class SI_CameraController : MonoBehaviour
{
    public static SI_CameraController Instance;

    Camera mainCamera;
    Camera playerCamera;

    //forgotten touch input related stuff
    Vector2 touchDeltaPosition;
    Vector2 touchZeroDelta;
    Vector2 touchOneDelta;

    float prevTouchDeltaMag = 0;
    float touchDeltaMag = 0;
    float deltaMagnitudeDiff = 0;

    //custom camera bounds
    [SerializeField] bool outOfBounds; 
    [SerializeField] Vector2 mapVerticalBounds; //-5, 33
    Vector2 calculatedBounds;
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

    Vector3 zoomTemp;
    Vector3 dragTemp;
    Vector3 panTemp;
    Vector3 dir;
    Vector3 lastCameraPosition;

    public LayerMask menuLayerMask;
    public LayerMask gameLayerMask;
    public bool animationsRunning;

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

        calculatedBounds = mapVerticalBounds;
        oldPosition = this.transform.position;
        mainCamera = transform.GetChild(0).GetComponent<Camera>();
        playerCamera = transform.GetChild(0).GetChild(0).GetComponent<Camera>();
        


        mainCamera.cullingMask = menuLayerMask;
        mainCamera.gameObject.SetActive(true);
        playerCamera.gameObject.SetActive(false);

        zoomTemp = this.transform.position;
        if (zoomTemp.y < minHeight)
        {
            zoomTemp.y = minHeight;
        }
        if (zoomTemp.y > maxHeight)
        {
            zoomTemp.y = maxHeight;
        }

        int m = (19 - 33) / (20 - 10); // Calculate the slope (change in y / change in x)
        int c = 33 - m * 10; // Calculate the y-intercept
        calculatedBounds.y = m * zoomTemp.y + c;

    }

    public void GameStarted()
    {
        mainCamera.cullingMask = gameLayerMask;
        playerCamera.gameObject.SetActive(true);
        Update_CurrentFunction = Update_DetectModeStart;
    }

    public void GameEnded()
    {
        mainCamera.cullingMask = menuLayerMask;
        playerCamera.gameObject.SetActive(false);
    }
  
    private void Update()
    {
        if (!GameManager.Instance.gameReady)
        {
            return;
        }

        if (UnitManager.Instance.runningCombatSequence || UnitManager.Instance.runningMoveSequence
        || MapManager.Instance.upgradingCity || MapManager.Instance.cityIsWorking || MapManager.Instance.occupyingCity)
        {
            animationsRunning = true;
        }
        else
        {
            animationsRunning = false;
        }

        if (autoMove)
        {
            AutoHexPan();
            CheckifCameraMoved();
            return;
        }

        if (outOfBounds)
        {
          // MoveBack();
           // return;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) || !Application.isFocused)
        {
            CancelUpdateFunction();
            return;
        }

        Update_CurrentFunction();

        if (!IsPointerOverUIObject())
        {
            

            if (zoomEnabled)
            {
                Update_ScrollZoom();
            }

        }

        if (keyboardControls)
        {
            KeyboardInput();
        }

        if (touchControls)
        {
           // TouchInput();
        }

        lastMousePosition = Input.mousePosition;
        lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
        CheckifCameraMoved();
    }

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

                touchDeltaPosition = Input.GetTouch(0).deltaPosition;
                transform.Translate(-touchDeltaPosition.x * 1, -(touchDeltaPosition.y * 1), 0);

                dragTemp = this.transform.position;
                if (dragTemp.z < calculatedBounds.x)
                {
                    dragTemp.z = calculatedBounds.x;
                }
                if (dragTemp.z > calculatedBounds.y)
                {
                    dragTemp.z = calculatedBounds.y;
                }

                this.transform.position = dragTemp;

                lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);

                OutOfBoundsCheck();
            }

            if (Input.touchCount == 2)
            {
                tapValid = false;

                if (!IsMouseOverGameWindow)
                {
                    return;
                }


                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                touchZeroDelta = touchZero.position - touchZero.deltaPosition;
                touchOneDelta = touchOne.position - touchOne.deltaPosition;

                prevTouchDeltaMag = (touchZeroDelta - touchOneDelta).magnitude;
                touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                // Zoom to scrollwheel
                float scrollAmount = deltaMagnitudeDiff * 1;

                dir = hitPos - this.transform.position;

                zoomTemp = this.transform.position;

                // Stop zooming out at a certain distance.
                // TODO: Maybe you should still slide around at 20 zoom?
                if (scrollAmount > 0 || zoomTemp.y < (maxHeight - 0.1f))
                {
                    cameraTargetOffset += dir * scrollAmount;
                }

                lastCameraPosition = this.transform.position;
                this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + cameraTargetOffset, Time.deltaTime * 5f);
                cameraTargetOffset -= this.transform.position - lastCameraPosition;


                zoomTemp = this.transform.position;
                if (zoomTemp.y < minHeight)
                {
                    zoomTemp.y = minHeight;
                }
                if (zoomTemp.y > maxHeight)
                {
                    zoomTemp.y = maxHeight;
                }

                int m = (19 - 33) / (20 - 10); // Calculate the slope (change in y / change in x)
                int c = 33 - m * 10; // Calculate the y-intercept
                calculatedBounds.y = m * zoomTemp.y + c;

                if (zoomTemp.z < calculatedBounds.x)
                {
                    zoomTemp.z = calculatedBounds.x;
                }
                if (zoomTemp.z > calculatedBounds.y)
                {
                    zoomTemp.z = calculatedBounds.y;
                }

                this.transform.position = zoomTemp;

                // Change camera angle
                this.transform.rotation = Quaternion.Euler(
                    Mathf.Lerp(60, 60, Camera.main.transform.position.y / maxHeight),
                    Camera.main.transform.rotation.eulerAngles.y,
                    Camera.main.transform.rotation.eulerAngles.z
                );

                OutOfBoundsCheck();

            }

            if (Input.touches[0].phase == TouchPhase.Ended && tapValid)
            {
                Update_Tap();
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
        OutOfBoundsCheck();

        if (targetCameraPosition.z < calculatedBounds.x)
        {
            targetCameraPosition.z = calculatedBounds.x;
        }
        if (targetCameraPosition.z > calculatedBounds.y)
        {
            targetCameraPosition.z = calculatedBounds.y;
        }

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

        OutOfBoundsCheck();
    }

    void OutOfBoundsCheck()
    {
        if (transform.position.z < calculatedBounds.x || transform.position.z > calculatedBounds.y ) //y max, x min
        {
            outOfBounds = true;
            targetCameraPosition = this.transform.position;
            targetCameraPosition.z = Mathf.Clamp(transform.position.z, calculatedBounds.x + 1, calculatedBounds.y - 1);

            return;
        }
    }

    void MoveBack()
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, targetCameraPosition, ref currentVelocity, smoothTime);

        if (Vector3.Distance(this.transform.position, targetCameraPosition) <= 0.1)
        {
            outOfBounds = false;
            currentVelocity = Vector3.zero;
        }

        CheckifCameraMoved();
    }
    void Update_Tap()
    {
        if (IsPointerOverUIObject())
        {
            Debug.Log("Mouse is over GUI");
            return;
        }

        if(animationsRunning)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableMask))
        {
            if (hit.transform.CompareTag("Tile"))
            {
                Debug.Log("Mouse is over Tile");
                WorldHex newTile = hit.transform.parent.parent.gameObject.GetComponent<WorldHex>();
                UIManager.Instance.HideResearchPanel();
                UIManager.Instance.HideOverviewPanel();
                UIManager.Instance.HideSettingsPanel();
                SelectTile(newTile);
                //PanToHex(newTile);
            }
            else
            {
                Debug.Log("Mouse is over " + hit.transform.tag);
                selectedTile = null;
            }
        }
        else
        {
            Debug.Log("Mouse did not hit");
            selectedTile = null;
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

        dragTemp = this.transform.position;
        if (dragTemp.z < calculatedBounds.x)
        {
            dragTemp.z = calculatedBounds.x;
        }
        if (dragTemp.z > calculatedBounds.y)
        {
            dragTemp.z = calculatedBounds.y;
        }
        this.transform.position = dragTemp;

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);

        OutOfBoundsCheck();
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

    bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    void Update_ScrollZoom()
    {
        if (!IsMouseOverGameWindow)
        {
            return;
        }
        // Zoom to scrollwheel
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");

        // Move camera towards hitPos
        hitPos = MouseToGroundPlane(Input.mousePosition);
        dir = hitPos - this.transform.position;
        
        zoomTemp = this.transform.position;

        // Stop zooming out at a certain distance.
        // TODO: Maybe you should still slide around at 20 zoom?
        if (scrollAmount > 0 || zoomTemp.y < (maxHeight - 0.1f))
        {
            cameraTargetOffset += dir * scrollAmount;
        }
        lastCameraPosition = this.transform.position;
        this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + cameraTargetOffset, Time.deltaTime * 5f);
        cameraTargetOffset -= this.transform.position - lastCameraPosition;


        zoomTemp = this.transform.position;
        if (zoomTemp.y < minHeight)
        {
            zoomTemp.y = minHeight;
        }
        if (zoomTemp.y > maxHeight)
        {
            zoomTemp.y = maxHeight;
        }

        int m = (19 - 33) / (20 - 10); // Calculate the slope (change in y / change in x)
        int c = 33 - m * 10; // Calculate the y-intercept
        calculatedBounds.y = m * zoomTemp.y + c; 

        if (zoomTemp.z < calculatedBounds.x)
        {
            zoomTemp.z = calculatedBounds.x;
        }
        if (zoomTemp.z > calculatedBounds.y)
        {
            zoomTemp.z = calculatedBounds.y;
        }

        this.transform.position = zoomTemp;

        // Change camera angle
        this.transform.rotation = Quaternion.Euler(
            Mathf.Lerp(60, 60,Camera.main.transform.position.y / maxHeight),
            Camera.main.transform.rotation.eulerAngles.y,
            Camera.main.transform.rotation.eulerAngles.z
        );

        OutOfBoundsCheck();

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

    public void DeselectSelection()
    {
        if (selectedTile != null)
        {
            selectedTile.Deselect();
        }

        selectedTile = null;
    }

    public void SelectTile(WorldHex newTile) //this shouldnt be public. Used for cityView workaround
    {
        if (selectedTile != null)
        {
            selectedTile.Deselect();
        }
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


#if UNITY_IOS || UNITY_ANDROID

#endif
}
