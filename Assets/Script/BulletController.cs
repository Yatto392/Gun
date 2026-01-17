using UnityEngine;

public class BulletController : MonoBehaviour
{
    // --- Homing Settings ---
    private GameObject _target = null;
    public float speed = 15.0f;    // 1秒間に進む距離
    public float rotSpeed = 360.0f; // 1秒間に回転する角度

    // --- Particle Trail Settings ---

    private GameObject particleInstance;
    private ParticleSystem ps;

    // --- Public Property for Target ---
    public GameObject Target
    {
        set { _target = value; }
        get { return _target; }
    }


    void Update()
    {
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
        // パーティクルインスタンス（残像）が存在する場合の処理
        if (particleInstance != null)
        {
            particleInstance.transform.SetParent(null); // 親子関係を解除

            if (ps != null)
            {
                ps.Stop(); // 新しいパーティクルの放出を停止
                Destroy(particleInstance, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(particleInstance, 3f);
            }
        }

        // 弾丸オブジェクト自身を破棄
        Destroy(gameObject);
    }
}