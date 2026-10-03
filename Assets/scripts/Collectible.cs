using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    public int points = 10;
    public bool isOptional = false;
    public float rotateSpeed = 90f;
    public float bobHeight = 0.15f;
    public float bobSpeed = 2f;

    Vector3 startPos;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        if (LevelManager.Instance != null)
            LevelManager.Instance.AddCollectible(points, isOptional);
        Destroy(gameObject);
    }
}