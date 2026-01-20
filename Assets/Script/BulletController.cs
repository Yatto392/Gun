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

    private Rigidbody rb; // Rigidbodyへの参照を追加

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbodyコンポーネントを取得
        Debug.Log($"Bullet Rigidbody found: {rb != null}"); // Rigidbodyの取得状況をログ出力

        if (rb == null)
        {
            Debug.LogError("BulletController: Rigidbody component not found on this GameObject.", this);
            // Rigidbodyがない場合は処理を中断するか、Rigidbodyを追加するなどの対応が必要
            return;
        }

        // 弾丸に初速を与える
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject,5); // 弾丸の寿命を5秒に延長
    }

    void Update()
    {
        // Rigidbodyで移動を制御するため、Updateでのtransform操作は削除
        // 必要であれば、ここでRigidbodyを使った追加の力などを加える
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