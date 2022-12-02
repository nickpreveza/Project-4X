using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SI_CameraController : MonoBehaviour
{
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
    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        playerCamera = transform.GetChild(0).GetComponent<Camera>();
    }
    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (Input.touchCount < 2 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
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
                if (transform.position.x > xBounds.y || transform.position.x < xBounds.x)
                {
                    outOfBoundsX = true;
                }

                if (transform.position.y > yBounds.y || transform.position.y < yBounds.x)
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
            editedPosition.x = Mathf.Clamp(transform.position.x, xBounds.x, xBounds.y);
            editedPosition.y = Mathf.Clamp(transform.position.y, yBounds.x, yBounds.y);
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

    IEnumerator ReturnToBounds()
    {
      
        yield return new WaitForSeconds(0.1f);
        editedPosition.x = Mathf.Clamp(transform.position.x, xBounds.x, xBounds.y);
        editedPosition.y = Mathf.Clamp(transform.position.y, yBounds.x, yBounds.y);
        editedPosition.z = 0;// Mathf.Clamp(transform.position.z, zBounds.x, zBounds.y);
        movingBack = true;
       
    }
#if UNITY_IOS || UNITY_ANDROID

#endif
}
