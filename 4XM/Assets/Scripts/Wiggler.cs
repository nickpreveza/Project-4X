using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggler : MonoBehaviour
{
    Vector3 initPos, currentPosition, targetPosition;
    [Header("Animations")]
   
    float animationTime;
    float timeElapsed;
    [SerializeField] Vector3[] targetPoints;
    [SerializeField] float[] animationTimes;
    int targetIndex;

    Vector3 targetPoint;
    bool wiggling;
    bool moving;
    void Start()
    {
        initPos = transform.localPosition;
        currentPosition = initPos;
    }

    public void AnimatedMove(Vector3 targetPoint)
    {
        initPos = transform.localPosition;
        targetPoint.y = 1;
        SetDestination(targetPoint,.5f, true);
       
    }

    public void Wiggle()
    {
        if (wiggling)
        {
            return;
        }
        initPos = transform.localPosition;
        targetIndex = 0;
        SetDestination(targetPoints[targetIndex], animationTimes[targetIndex], false);
        wiggling = true;
    }

    void SetDestination(Vector3 destination, float time, bool useWorldPosition)
    {
        timeElapsed = 0;
        animationTime = time;
        if (useWorldPosition)
        {
            currentPosition = transform.localPosition;
            targetPosition = destination;
            moving = true;
        }
        else
        {
            currentPosition = transform.localPosition;
            targetPosition = initPos + destination;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (wiggling)
        {
            timeElapsed += Time.deltaTime / animationTime;
            transform.localPosition = Vector3.Lerp(currentPosition, targetPosition, timeElapsed);

            if (Vector3.Distance(transform.localPosition, targetPosition) <= 0.001f)
            {
                if (targetIndex + 1 >= targetPoints.Length)
                {
                    wiggling = false;
                }
                else
                {
                    targetIndex++;
                    SetDestination(targetPoints[targetIndex], animationTimes[targetIndex], false);
                }

            }
        }

        if (moving)
        {
            timeElapsed += Time.deltaTime / animationTime;
            transform.localPosition = Vector3.Lerp(currentPosition, targetPosition, timeElapsed);

            if (Vector3.Distance(transform.localPosition, targetPosition) <= 0.001f)
            {
                moving = false;
                initPos = transform.localPosition;
            }
        }
    }
}
