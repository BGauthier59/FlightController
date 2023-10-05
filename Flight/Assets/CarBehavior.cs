using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CarBehavior : MonoBehaviour
{

    public int previous,next;
    public float speed, distBetweenNodes,index;
    public List<Vector3> distancedNodes;
    public float completion;
    public float upVector = 1.2f;
  

    void Update()
    {
        completion += Time.deltaTime * speed;
        if (completion >= 1)
        {
            completion--;
        }
        
        index = Mathf.Lerp(0, distancedNodes.Count, completion);
        previous = Mathf.FloorToInt(index);
        next = Mathf.CeilToInt(index);
        if (next > distancedNodes.Count - 1)
        {
            next = 1;
            previous = 0;
        }
        index -= previous;
        
        transform.position = Vector3.Lerp(distancedNodes[previous], distancedNodes[next], index)+Vector3.up*upVector;
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(distancedNodes[next] - distancedNodes[previous]),Time.deltaTime*5);
        
    }
}
