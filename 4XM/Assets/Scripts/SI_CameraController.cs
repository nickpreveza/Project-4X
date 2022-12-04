using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SI_CameraController : MonoBehaviour
{
    public static SI_CameraController Instance;
   
    [SerializeField] float scrollSpeed = 0.01f;
    [SerializeField] float zoomSpeed = 0.01f;
    Vector2 touchDeltaPosition;
    public Vector2 xBounds;
    public Vector2 yBounds;
    public Vector2 zBounds;
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
    WorldTile selectedTile;

    [SerializeField] float internalTouchTimer;
    [SerializeField] float timeToRegisterTap;
    [SerializeField] float timeToRegisterHold;

    bool tapValid;
    int layerAccessor;
    [SerializeField] int internalMapHeight;
    [SerializeField] int internalMapWidth;
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

        mainCamera = transform.GetChild(0).GetComponent<Camera>();
        playerCamera = transform.GetChild(0).GetChild(0).GetComponent<Camera>();
        internalBoundsX = xBounds;
        internalBoundsY = yBounds;
    }

    public void UpdateBounds(int mapWidth, int mapHeight)
    {
        internalMapHeight = mapHeight;
        internalMapWidth = mapWidth;
        UpdateBounds();
    }

    public void UpdateBounds()
    {
        internalBoundsX.x = xBounds.x - internalMapHeight;
        internalBoundsX.y = internalMapWidth - xBounds.y;
        internalBoundsY.x = yBounds.x;
        internalBoundsY.y = (yBounds.y + xBounds.x) + internalMapHeight;
    }

    void Update()
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
                    if (hit.transform.CompareTag("Unit"))
                    {
                        WorldTile newTile = hit.transform.gameObject.GetComponent<WorldUnit>().parentTile;
                        SelectTile(newTile);
                    }
                    if (hit.transform.CompareTag("Tile"))
                    {
                        if (internalTouchTimer > timeToRegisterTap && internalTouchTimer < timeToRegisterHold)
                        {
                            WorldTile newTile = hit.transform.gameObject.GetComponent<WorldTile>();
                            SelectTile(newTile);
                            return;
                        }

                        if (internalTouchTimer >= timeToRegisterHold)
                        {
                            selectedTile = hit.transform.gameObject.GetComponent<WorldTile>();
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
                            selectedTile = hit.transform.gameObject.GetComponent<WorldTile>();
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

    void SelectTile(WorldTile newTile)
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
