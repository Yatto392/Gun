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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // --- Target Finding Logic ---
            GameObject bestTarget = null;
            float smallestAngle = 180.0f; // Start with a large angle

            // Find all enemies
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in enemies)
            {
                Vector3 dirToEnemy = enemy.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, dirToEnemy);

                // Check if the enemy is in front and the angle is the smallest so far
                if (angle < smallestAngle)
                {
                    smallestAngle = angle;
                    bestTarget = enemy;
                }
            }
            
            // --- Fire Bullet ---
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            BulletController bulletCtrl = bullet.GetComponent<BulletController>();

            if (bulletCtrl != null)
            {
                // If a target was found, assign it
                if (bestTarget != null)
                {
                    bulletCtrl.Target = bestTarget;
                }
                // If no target is found, the bullet will fly straight
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
