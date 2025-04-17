using UnityEngine;
using TMPro;

public class GoalController : MonoBehaviour
{
    public TextMeshPro goalText;
    private int goalCount = 0;

    private bool canScore = true;
    public float scoreCooldown = 2f; 

    private void OnTriggerEnter(Collider other)
    {
        if (canScore && other.CompareTag("Ball"))
        {
            goalCount++;
            goalText.text = goalCount.ToString();
            StartCoroutine(ScoreCooldown());
        }
    }

    private System.Collections.IEnumerator ScoreCooldown()
    {
        canScore = false;
        yield return new WaitForSeconds(scoreCooldown);
        canScore = true;
    }
}
