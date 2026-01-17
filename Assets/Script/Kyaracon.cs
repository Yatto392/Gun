using System.Collections.Generic;
using UnityEngine;

public class Kyaracon : MonoBehaviour
{
    public List<GameObject> targetObjects;
    private int currentIndex = 0;
    public GameObject bulletPrefab; // 弾丸のプレハブ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetObjects != null && targetObjects.Count > 0)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = targetObjects[currentIndex].transform.position.x;
            transform.position = newPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
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

        // Aキーで前のオブジェクトへ移動 (インデックス減少)
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                Vector3 newPosition = transform.position;
                newPosition.x = targetObjects[currentIndex].transform.position.x;
                transform.position = newPosition;
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
            }
        }
    }
}
