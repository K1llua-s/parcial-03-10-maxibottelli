using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    public Transform respawnPoint;
    public Renderer indicator;
    public Color activeColor = Color.green;

    bool activated;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated || other.GetComponentInParent<PlayerMovement>() == null) return;
        activated = true;

        Vector3 pos = respawnPoint != null ? respawnPoint.position : transform.position + Vector3.up;
        if (LevelManager.Instance != null) LevelManager.Instance.SetCheckpoint(pos);
        if (indicator != null) indicator.material.color = activeColor;
    }
}