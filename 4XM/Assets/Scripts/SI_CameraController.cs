using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    WorldHex selectedTile;

    [SerializeField] float internalTouchTimer;
    [SerializeField] float timeToRegisterTap;
    [SerializeField] float timeToRegisterHold;

    bool tapValid;
    int layerAccessor;


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

    void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (mouseRay.direction.y >= 0)
        {
            Debug.LogError("Why is mouse pointing up?");
            return;
        }
        rayLenght = mouseRay.origin.y / mouseRay.direction.y;
        hitPos = mouseRay.origin + (mouseRay.direction * rayLenght);

        MouseInput();

        if (touchControls)
        {
            TouchInput();
        }

        if (keyboardControls)
        {
            KeyboardInput();
        }
        if (zoomEnabled)
        {
            ZoomInput();
            //Update_ScrollZoom();
        }

        if (dragEnabled)
        {
            DragInput(mouseRay);
        }
      


        CheckifCameraMoved();
    }


    public void PanToHex(Hex hex)
    {
        //TODO: Move camera to hex
    }


    void KeyboardInput()
    {
        Vector3 translate = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        this.transform.Translate(translate * moveSpeed * Time.deltaTime, Space.World);
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
    }

    void MouseInput()
    {
        //Ray
      

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Tile"))
                {
                    WorldHex newTile = hit.transform.parent.parent.gameObject.GetComponent<WorldHex>();
                    SelectTile(newTile);
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


    void DragInput(Ray mouseRay)
    {
        //Dragging
        if (Input.GetMouseButtonDown(1))
        {
            isDraggingCamera = true;

            lastMousePosition = hitPos;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isDraggingCamera = false;
        }

        if (isDraggingCamera)
        {
            diff = lastMousePosition - hitPos;
            this.transform.Translate(diff, Space.World);
            mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (mouseRay.direction.y >= 0)
            {
                Debug.LogError("Why is mouse pointing up?");
                return;
            }

            rayLenght = mouseRay.origin.y / mouseRay.direction.y;
            lastMousePosition = mouseRay.origin + (mouseRay.direction * rayLenght);
        }
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
            }
               
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
        float minHeight = 2;
        float maxHeight = 20;
        // Move camera towards hitPos
        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);
        Vector3 dir = hitPos - Camera.main.transform.position;

        Vector3 p = Camera.main.transform.position;

        // Stop zooming out at a certain distance.
        // TODO: Maybe you should still slide around at 20 zoom?
        if (scrollAmount > 0 || p.y < (maxHeight - 0.1f))
        {
            cameraTargetOffset += dir * scrollAmount;
        }
        Vector3 lastCameraPosition = Camera.main.transform.position;
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, Camera.main.transform.position + cameraTargetOffset, Time.deltaTime * 5f);
        cameraTargetOffset -= Camera.main.transform.position - lastCameraPosition;


        p = Camera.main.transform.position;
        if (p.y < minHeight)
        {
            p.y = minHeight;
        }
        if (p.y > maxHeight)
        {
            p.y = maxHeight;
        }
        Camera.main.transform.position = p;

        // Change camera angle
        Camera.main.transform.rotation = Quaternion.Euler(
            Mathf.Lerp(30, 75, Camera.main.transform.position.y / maxHeight),
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
            layerAccessor = 2;
        }
        else
        {
            layerAccessor = 1;
        }
        selectedTile = newTile;
        if (selectedTile != null)
        {
            selectedTile.Tap(layerAccessor);
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
