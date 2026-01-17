using UnityEngine;

public class BulletController : MonoBehaviour
{
    // --- State Machine ---
    private enum BulletState { Ascent, Homing }
    private BulletState currentState = BulletState.Ascent;
    private float ascentTimer = 0.0f;
    public float ascentDuration = 0.5f; // 上昇する時間
    public float ascentSpeed = 20.0f;   // 上昇速度

    // --- Homing Settings ---
    private GameObject _target = null;
    public float speed = 15.0f;    // 1秒間に進む距離
    public float rotSpeed = 360.0f; // 1秒間に回転する角度

    // --- Public Property for Target ---
    public GameObject Target
    {
        set { _target = value; }
        get { return _target; }
    }
    
    void Update()
    {
        switch (currentState)
        {
            case BulletState.Ascent:
                // --- Ascent Logic ---
                transform.position += Vector3.up * ascentSpeed * Time.deltaTime;
                ascentTimer += Time.deltaTime;
                if (ascentTimer >= ascentDuration)
                {
                    currentState = BulletState.Homing;
                }
                break;

            case BulletState.Homing:
                // --- Homing Logic ---
                if (_target != null)
                {
                    // ターゲットまでの角度を取得
                    Vector3 vecTarget = _target.transform.position - transform.position;
                    Vector3 vecForward = transform.TransformDirection(Vector3.forward);
                    float angleDiff = Vector3.Angle(vecForward, vecTarget);
                    float angleAdd = (rotSpeed * Time.deltaTime);
                    Quaternion rotTarget = Quaternion.LookRotation(vecTarget);

                    if (angleDiff <= angleAdd)
                    {
                        transform.rotation = rotTarget;
                    }
                    else
                    {
                        float t = (angleAdd / angleDiff);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotTarget, t);
                    }
                }

                // --- Forward Movement ---
                transform.position += transform.TransformDirection(Vector3.forward) * speed * Time.deltaTime;
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // "wall" または "Enemy" タグを持つオブジェクトに衝突した場合
        if (collision.gameObject.CompareTag("wall") || collision.gameObject.CompareTag("Enemy"))
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