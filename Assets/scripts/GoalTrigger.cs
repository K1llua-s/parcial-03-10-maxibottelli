using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("GoalTrigger tocado por: " + other.name);

        if (other.GetComponentInParent<PlayerMovement>() == null) return;

        Debug.Log("Es el jugador. LevelManager: " + (LevelManager.Instance != null));
        if (LevelManager.Instance != null) LevelManager.Instance.ShowWin();
    }
}