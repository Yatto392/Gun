using UnityEngine;

public class BulletController : MonoBehaviour
{
    // --- State Machine ---
    private enum BulletState { Ascent, Homing } // Homing state will now represent straight flight
    private BulletState currentState = BulletState.Homing; // Start directly in straight flight state

    // --- Straight Flight Settings ---
    public float speed = 15.0f;    // 1秒間に進む距離
    // rotSpeed, avoidanceDistance, avoidanceAngle and _target are no longer needed for straight flight.
    // However, keeping them to minimize structural changes if not explicitly requested to remove members.
    private GameObject _target = null; // No longer used for homing
    public float rotSpeed = 360.0f; // No longer used for homing
    public float avoidanceDistance = 5.0f; // No longer used for avoidance
    public float avoidanceAngle = 45.0f; // No longer used for avoidance

    // --- Public Property for Target ---
    // Target property kept for compatibility, but its value won't influence flight path

    
    public GameObject Target
    {
        set { _target = value; }
        get { return _target; }
    }

    void Start()
    {
        Destroy(gameObject,10);
    }

    void Update()
    {
        switch (currentState)
        {
            case BulletState.Homing: // Now functions as Straight Flight
                // --- Straight Flight Logic (XY Plane) ---
                // Simply move forward along current transform.forward, constrained to XY plane

                // 1. 前進 (Z座標を固定)
                float originalZ = transform.position.z;
                transform.position += transform.forward * speed * Time.deltaTime;
                Vector3 newPos = transform.position;
                newPos.z = originalZ;
                transform.position = newPos;
                break;
        }
    }

    // FindAvoidancePath is no longer used.
    // Keeping it for now to minimize structural changes unless explicitly requested to remove members.
    Vector3 FindAvoidancePath(Vector3 currentDirection)
    {
        return currentDirection; // Should not be called in straight flight mode
    }

    void OnTriggerEnter(Collider other)
    {
        // "wall" または "Enemy" タグを持つオブジェクトに衝突した場合
        if (other.gameObject.CompareTag("wall") || other.gameObject.CompareTag("Enemy"))
        {
            HandleDestruction();
        }
    }

    void HandleDestruction()
    {
        // 弾丸オブジェクト自身を破棄
        Destroy(gameObject);
    }
    
}