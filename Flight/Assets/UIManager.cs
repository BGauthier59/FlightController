using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Transform player,arrow;
    [SerializeField] private Transform goal;
    [SerializeField] private TMP_Text meterCount;

    // Update is called once per frame
    void Update()
    {
        arrow.rotation = Quaternion.Lerp(arrow.rotation,Quaternion.LookRotation((goal.position - player.position).normalized),Time.deltaTime*5);
        meterCount.text = Mathf.Round(Vector3.Distance(goal.position, player.position)) + "m";
    }
}
