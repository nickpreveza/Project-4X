using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    public float baseSpeed;
    private float height;
    [Range(0, 1)] public float arcHeight;
    public int bounceCount;
    [SerializeField] bool flying;
    [SerializeField] float progress;
    private float stepSize;
    private float distance;
    private Vector2 dir;
    [SerializeField] Vector3 targetPos;
    [SerializeField] Vector3 startPos;
    [SerializeField] bool arrived = false;
    private int bounceIndex = 0;
    private bool stopBouncing;
    private List<Vector3> targets = new List<Vector3>();
    internal float speed;
    [SerializeField] bool dropped;

    private void Start()
    {
        Drop();
    }
    void Update()
    {
        if (dropped)
        {
            if (!arrived)
            {
                gameObject.transform.position = ParabolicMovement(targets[bounceIndex], targets[bounceIndex + 1]);
            }
            if (arrived && flying)
            {
                OnArrival();
            }
        }
      
    }

    Vector2 ParabolicMovement(Vector3 _startPos, Vector3 target)
    {
        arcHeight = height * distance / (bounceIndex + 1);
        stepSize = speed / distance;
        flying = true;
        // Increment our progress from 0 at the start, to 1 when we arrive.
        progress = Mathf.Min(progress + Time.deltaTime * stepSize, 1.0f);
        // Turn this 0-1 value into a parabola that goes from 0 to 1, then back to 0.
        float parabola = 1.0f - 4.0f * (progress - 0.5f) * (progress - 0.5f);
        // Travel in a straight line from our start position to the target.        
        Vector3 nextPos = Vector3.Lerp(_startPos, target, progress);
        // Then add a vertical arc in excess of this.
        nextPos.y += parabola * arcHeight;

        if (progress >= 1.0f)
        {
            if (bounceIndex == targets.Count - 2)
            {
                arrived = true;
            }
            else if (bounceIndex < targets.Count - 2)
            {
                progress = 0;
                bounceIndex++;
            }
        }

        return nextPos;
    }

    public void Drop()
    {
        dropped = true;
        height = arcHeight;
        speed = baseSpeed;
        stopBouncing = false;
        targets.Clear();
        bounceIndex = 0;
        Vector3 randomizedPos = new Vector3(Random.Range(-1,-0.5f ), Random.Range(-1, 1), 0);
        startPos = transform.position;
        targetPos = startPos + randomizedPos;
        distance = Vector2.Distance(targetPos, startPos);
        progress = 0;
        dir = (targetPos - startPos).normalized;
        targets.Add(startPos);
        targets.Add(targetPos);

        for (int i = 0; i < bounceCount; i++)
        {
            Vector3 nextPos = targets[i + 1] + (Vector3)dir * distance / ((i + 1) * 2);
            RaycastHit2D hit = Physics2D.Raycast(targets[i + 1], targets[i + 1] - nextPos, Vector2.Distance(targets[i + 1], nextPos));
            if (hit.collider != null && hit.collider.CompareTag("Obstacles"))
            {
                break;
            }
            targets.Add(nextPos);
        }
        arrived = false;
       
    }

    private void OnArrival()
    {
        flying = false;
        GetComponent<Collider2D>().isTrigger = true;
       // GetComponent<Interactable>().isInteractable = true;
    }


}
