using UnityEngine;

public class ObjectTrigger : MonoBehaviour
{
    [Header("Settings")]
    public bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Notify the enemy AI to investigate this object
            HorrorEnemyAI.instance.InvestigateObject(transform);
            isTriggered = true;
        }
    }
}
