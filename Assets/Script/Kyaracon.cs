
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

    [Header("Camera Rotation")]
    public Camera mainCamera;
    public float rotationSpeed = 20f; // Degrees per second
    public float maxRotationY = 3.6f;

    private float previousX;
    private float _currentTargetYRotation;

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

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        previousX = transform.position.x;
        _currentTargetYRotation = 0f;
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
            int previousIndex = currentIndex - 1;
            if (previousIndex >= 0)
            {
                // Check if the IMMEDIATE previous target object is active
                if (targetObjects[previousIndex] != null && targetObjects[previousIndex].activeInHierarchy)
                {
                    currentIndex = previousIndex;
                    Vector3 newPosition = transform.position;
                    newPosition.x = targetObjects[currentIndex].transform.position.x;
                    transform.position = newPosition;
                    lastMoveTime = Time.time; // 移動時間を更新
                }
                // If the immediate previous object is inactive, do nothing (player is blocked)
            }
        }

        // Dキーで次のオブジェクトへ移動 (インデックス増加)
        if (Input.GetKeyDown(KeyCode.D))
        {
            int nextIndex = currentIndex + 1;
            if (nextIndex < targetObjects.Count)
            {
                // Check if the IMMEDIATE next target object is active
                if (targetObjects[nextIndex] != null && targetObjects[nextIndex].activeInHierarchy)
                {
                    currentIndex = nextIndex;
                    Vector3 newPosition = transform.position;
                    newPosition.x = targetObjects[currentIndex].transform.position.x;
                    transform.position = newPosition;  
                    lastMoveTime = Time.time; // 移動時間を更新
                }
                // If the immediate next object is inactive, do nothing (player is blocked)
            }
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // --- Camera Rotation Logic ---
        float currentX = transform.position.x;

        // Determine the target rotation based on movement
        if (Mathf.Abs(currentX - previousX) >= 0.001f) // If player is moving
        {
            if (currentX > previousX)
            {
                // Moved right
                _currentTargetYRotation = maxRotationY;
            }
            else // currentX < previousX (Moved left)
            {
                // Moved left
                _currentTargetYRotation = -maxRotationY;
            }
        }
        // If not moving, _currentTargetYRotation remains what it was.

        // Always rotate towards the _currentTargetYRotation
        Quaternion targetRotation = Quaternion.Euler(mainCamera.transform.eulerAngles.x, _currentTargetYRotation, mainCamera.transform.eulerAngles.z);
        mainCamera.transform.rotation = Quaternion.RotateTowards(mainCamera.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        previousX = currentX;
    }
}
