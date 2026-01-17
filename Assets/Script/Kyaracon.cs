using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを使用するために追加

public class Kyaracon : MonoBehaviour
{
    public List<GameObject> targetObjects;
    private int currentIndex = 0;
    public GameObject bulletPrefab; // 弾丸のプレハブ

    public float moveCooldown = 1.0f; // 移動のクールダウン時間（秒）
    private float lastMoveTime = -1.0f; // 最後に移動した時間
    public Image cooldownGauge; // クールダウン表示用のUI画像

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetObjects != null && targetObjects.Count > 0)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = targetObjects[currentIndex].transform.position.x;
            transform.position = newPosition;
        }

        // ゲージを最初に満タン状態にしておく
        if (cooldownGauge != null)
        {
            cooldownGauge.fillAmount = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // クールダウンゲージを更新
        if (cooldownGauge != null)
        {
            float timeSinceLastMove = Time.time - lastMoveTime;
            cooldownGauge.fillAmount = Mathf.Clamp01(timeSinceLastMove / moveCooldown);
        }
        
        // スペースキーで通常弾を発射
        if (Input.GetKeyDown(KeyCode.Space))
        {
                // --- Fire Normal Bullet ---
                GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                BulletController bulletCtrl = bullet.GetComponent<BulletController>();

                if (bulletCtrl != null)
                {
                    // ターゲットをセットしないので、まっすぐ飛ぶ
                    bulletCtrl.Target = null;
                }
        }

        if (targetObjects == null || targetObjects.Count == 0)
        {
            return;
        }

        // クールダウンが終了しているかチェック
        bool canMove = Time.time >= lastMoveTime + moveCooldown;
        
        if (!canMove) return; // クールダウン中なら何もしない

        // Aキーで前のオブジェクトへ移動 (インデックス減少)
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                Vector3 newPosition = transform.position;
                newPosition.x = targetObjects[currentIndex].transform.position.x;
                transform.position = newPosition;
                lastMoveTime = Time.time; // 移動時間を更新
            }
        }

        // Dキーで次のオブジェクトへ移動 (インデックス増加)
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentIndex < targetObjects.Count - 1)
            {
                currentIndex++;
                Vector3 newPosition = transform.position;
                newPosition.x = targetObjects[currentIndex].transform.position.x;
                transform.position = newPosition;  
                lastMoveTime = Time.time; // 移動時間を更新
            }
        }
    }
}
