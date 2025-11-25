using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TrialStartUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI uidText;
    public TextMeshProUGUI trialOrderText;
    public TextMeshProUGUI currentIndexText;

    [Header("Follow Settings")]
    public Transform head;
    public float followSpeed = 4f;
    public float distance = 1.4f;
    public float heightOffset = -0.2f;

    private GameController GC;
    private TrialController controller;
    private int maxCount;
    private List<DataManager.Conditons> orderList;

    public void Initialize(GameController gc, TrialController ctrl, int uid,
                           List<DataManager.Conditons> order, int currentIndex)
    {
        GC = gc;
        controller = ctrl;
        maxCount = order.Count;
        orderList = order;

        uidText.text = $"UID: {uid}";

        RebuildOrderDisplay(currentIndex);
        UpdateIndex(currentIndex);
    }

    public void UpdateIndex(int index)
    {
        // Update the index text
        currentIndexText.text = $"Trial: {index + 1}/{maxCount}";

        // Sync with GameController
        GC.currentConditionIndex = index;

        // Update the highlighted line
        RebuildOrderDisplay(index);
    }

    private void RebuildOrderDisplay(int highlightIndex)
    {
        trialOrderText.text = "";

        for (int i = 0; i < orderList.Count; i++)
        {
            bool isHighlighted = (i == highlightIndex);

            if (isHighlighted)
            {
                
                trialOrderText.text += $"<color=#36d40f><b>{i + 1}. {orderList[i]}</b></color>\n";

            }
            else
            {
                trialOrderText.text += $"{i + 1}. {orderList[i]}\n";
            }
        }
    }

    private void LateUpdate()
    {
        if (head == null) return;

        Vector3 targetPos = head.position +
                            head.forward * distance +
                            Vector3.up * heightOffset;

        transform.position = Vector3.Lerp(transform.position, targetPos,
                                          followSpeed * Time.deltaTime);

        // orient toward head yaw only
        Vector3 flatForward = new Vector3(head.forward.x, 0, head.forward.z);
        Quaternion targetRot = Quaternion.LookRotation(flatForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                              followSpeed * Time.deltaTime);
    }
}
