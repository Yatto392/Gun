
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CrouchMode { Toggle, Hold }

public class Kyaracon : MonoBehaviour
{
    public List<GameObject> targetObjects;
    private int currentIndex = 0;

    public GameObject bulletPrefab;
    public float fireCooldown = 0.5f;
    private float lastFireTime = -1.0f;

    public float moveCooldown = 1.0f;
    private float lastMoveTime = -1.0f;

    public Image cooldownGauge;

    [Header("Camera Rotation")]
    public Camera mainCamera;
    public float rotationSpeed = 40f;
    public float maxRotationY = 3.6f;
    private float previousX;
    private float _currentTargetYRotation;
    private float lockedZPosition;
    private float _targetCharacterYRotation;

    [Header("Ammo")]
    public int maxAmmo = 150;
    public int currentAmmo = 150;
    public int magazineSize = 30;
    public int currentMagazineAmmo;
    public float reloadTime = 1.5f;
    private bool isReloading = false;
    public TextMeshProUGUI ammoText;
    public Image ammoGauge;

    [Header("Bullet Spawn Point")]
    public Transform bulletSpawnPoint;

    [Header("Crouch Settings")]
    public Animator animator;
    public CrouchMode crouchMode = CrouchMode.Toggle;
    private const string syagamiParameter = "Syagami";
    private const string syagamiIdouParameter = "Syagami_idou";
    public float syagamiMoveDuration = 0.25f;
    private int _moveRequestIndex = -1; // しゃがみ移動リクエスト

    void Start()
    {
        if (targetObjects != null && targetObjects.Count > 0)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = targetObjects[currentIndex].transform.position.x;
            transform.position = newPosition;
        }

        if (cooldownGauge != null)
        {
            cooldownGauge.fillAmount = 1;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found on the GameObject or its children!", this);
            }
        }

        previousX = transform.position.x;
        _currentTargetYRotation = 0f;
        lockedZPosition = transform.position.z;
        _targetCharacterYRotation = transform.eulerAngles.y;

        currentMagazineAmmo = magazineSize;
        UpdateAmmoUI();
    }

    void Update()
    {
        HandleCrouchInput();

        if (isReloading)
        {
            return;
        }

        if (targetObjects != null && targetObjects.Count > 0)
        {
            if (currentIndex >= targetObjects.Count)
            {
                currentIndex = targetObjects.Count - 1;
            }

            GameObject currentPos = targetObjects[currentIndex];
            if (currentPos == null || !currentPos.activeInHierarchy)
            {
                FindAndMoveToNewPos();
            }
        }

        if (cooldownGauge != null)
        {
            float timeSinceLastMove = Time.time - lastMoveTime;
            cooldownGauge.fillAmount = Mathf.Clamp01(timeSinceLastMove / moveCooldown);
        }

        bool isCrouching = animator.GetBool(syagamiParameter);

        if (!isCrouching)
        {
            if (Input.GetKeyDown(KeyCode.R) && currentMagazineAmmo < magazineSize && currentAmmo > 0)
            {
                StartCoroutine(Reload());
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space key pressed.");

                bool canFire = Time.time >= lastFireTime + fireCooldown && currentMagazineAmmo > 0 && !isReloading;

                if (!canFire)
                {
                    Debug.LogWarning("Cannot fire!");
                    Debug.Log($"Time condition met: {Time.time >= lastFireTime + fireCooldown}");
                    Debug.Log($"Has ammo: {currentMagazineAmmo > 0} (Ammo: {currentMagazineAmmo})");
                    Debug.Log($"Not reloading: {!isReloading}");
                }
                else
                {
                    Fire();
                }

                if (currentMagazineAmmo <= 0 && !isReloading)
                {
                    Debug.Log("マガジンが空です！リロードしてください。");
                }
            }
        }

        if (targetObjects == null || targetObjects.Count == 0)
        {
            return;
        }

        bool canMove = Time.time >= lastMoveTime + moveCooldown;

        // 移動入力
        if (canMove && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)))
        {
            int direction = Input.GetKeyDown(KeyCode.A) ? -1 : 1;
            int targetIndex = currentIndex + direction;

            if (targetIndex >= 0 && targetIndex < targetObjects.Count &&
                targetObjects[targetIndex] != null && targetObjects[targetIndex].activeInHierarchy)
            {
                GameObject destination = targetObjects[targetIndex];
                GameObject currentPos = targetObjects[currentIndex];
                bool isSyagamiMove = destination.CompareTag("Syagami") || currentPos.CompareTag("Syagami");

                if (isSyagamiMove)
                {
                    if (isCrouching)
                    {
                        // しゃがみ移動リクエストを設定
                        _moveRequestIndex = targetIndex;
                        _targetCharacterYRotation = (direction == -1) ? -90f : 90f;
                    }
                    else
                    {
                        Debug.Log("Cannot move to/from Syagami point, not crouching.");
                    }
                }
                else
                {
                    if (!isCrouching)
                    {
                        _targetCharacterYRotation = (direction == -1) ? -90f : 90f;
                        MoveTo(targetIndex);
                    }
                }
            }
        }

        // しゃがみ移動リクエストの処理
        if (_moveRequestIndex != -1 && isCrouching)
        {
            float currentYAngle = transform.eulerAngles.y;
            // 回転が完了しているかチェック
            if (Mathf.Abs(Mathf.DeltaAngle(currentYAngle, _targetCharacterYRotation)) < 1.0f)
            {
                // クールダウンを再度チェック
                if (Time.time >= lastMoveTime + moveCooldown)
                {
                    StartCoroutine(MoveWithAnimation(_moveRequestIndex));
                    _moveRequestIndex = -1; // リクエストを消費したのでリセット
                }
            }
        }
        else if (_moveRequestIndex != -1 && !isCrouching)
        {
            // しゃがんでいない場合はリクエストをキャンセル
            _moveRequestIndex = -1;
        }
    }

    private void HandleCrouchInput()
    {
        if (animator == null) return;

        switch (crouchMode)
        {
            case CrouchMode.Hold:
                if (Input.GetKeyDown(KeyCode.S))
                {
                    animator.SetBool(syagamiParameter, true);
                }
                else if (Input.GetKeyUp(KeyCode.S))
                {
                    animator.SetBool(syagamiParameter, false);
                }
                break;

            case CrouchMode.Toggle:
                if (Input.GetKeyDown(KeyCode.S))
                {
                    bool isCrouching = animator.GetBool(syagamiParameter);
                    animator.SetBool(syagamiParameter, !isCrouching);
                }
                break;
        }
    }

    void Fire()
    {
        Debug.Log("Fire() method called. Instantiating bullet.");
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not set in the inspector!");
            return;
        }

        if (bulletSpawnPoint == null)
        {
            Debug.LogError("Bullet Spawn Point is not set in the inspector! Cannot fire bullet.", this);
            return;
        }

        currentMagazineAmmo--;
        lastFireTime = Time.time;
        UpdateAmmoUI();

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        BulletController bulletCtrl = bullet.GetComponent<BulletController>();

        if (bulletCtrl != null)
        {
            bulletCtrl.Target = null;
        }
    }

    IEnumerator Reload()
    {
        animator.SetTrigger("Gun_Reload");
        isReloading = true;
        Debug.Log("リロード中...");

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = magazineSize - currentMagazineAmmo;
        int ammoToReload = Mathf.Min(neededAmmo, currentAmmo);

        currentMagazineAmmo += ammoToReload;
        currentAmmo -= ammoToReload;

        isReloading = false;
        UpdateAmmoUI();
        Debug.Log("リロード完了！");
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentMagazineAmmo} / {currentAmmo}";
        }

        if (ammoGauge != null)
        {
            if (magazineSize > 0)
            {
                ammoGauge.fillAmount = (float)currentMagazineAmmo / magazineSize;
            }
        }
    }

    IEnumerator MoveWithAnimation(int newIndex)
    {
        lastMoveTime = Time.time;
        animator.SetBool(syagamiIdouParameter, true);

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(targetObjects[newIndex].transform.position.x, transform.position.y, transform.position.z);
        float elapsedTime = 0f;

        while (elapsedTime < syagamiMoveDuration)
        {
            float newX = Mathf.Lerp(startPosition.x, targetPosition.x, elapsedTime / syagamiMoveDuration);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Ensure it reaches the exact position
        currentIndex = newIndex;

        animator.SetBool(syagamiIdouParameter, false);
        animator.SetBool(syagamiParameter, false); // しゃがみ移動終了後、しゃがみ状態を解除
    }

    private void MoveTo(int newIndex, bool updateCooldown = true)
    {
        currentIndex = newIndex;
        Vector3 newPosition = transform.position;
        newPosition.x = targetObjects[currentIndex].transform.position.x;
        transform.position = newPosition;

        if (updateCooldown)
        {
            lastMoveTime = Time.time;
        }
    }

    private void FindAndMoveToNewPos()
    {
        int previousIndex = currentIndex - 1;
        if (previousIndex >= 0 && targetObjects[previousIndex] != null && targetObjects[previousIndex].activeInHierarchy)
        {
            MoveTo(previousIndex);
            Debug.Log($"Moved left to index {previousIndex} as current position became inactive (preferred).");
            return;
        }

        int nextIndex = currentIndex + 1;
        if (nextIndex < targetObjects.Count && targetObjects[nextIndex] != null && targetObjects[nextIndex].activeInHierarchy)
        {
            MoveTo(nextIndex);
            Debug.Log($"Moved right to index {nextIndex} as current position became inactive.");
            return;
        }

        Debug.LogWarning("Could not find an active position to move to!");
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (pos.z != lockedZPosition)
        {
            pos.z = lockedZPosition;
            transform.position = pos;
        }

        Quaternion currentCharacterRotation = transform.rotation;
        Quaternion targetCharacterRotation = Quaternion.Euler(
            currentCharacterRotation.eulerAngles.x,
            _targetCharacterYRotation,
            currentCharacterRotation.eulerAngles.z
        );

        transform.rotation = Quaternion.RotateTowards(
            currentCharacterRotation,
            targetCharacterRotation,
            rotationSpeed * Time.deltaTime
        );

        if (mainCamera == null) return;

        float currentX = transform.position.x;

        if (Mathf.Abs(currentX - previousX) >= 0.001f)
        {
            if (currentX > previousX)
            {
                _currentTargetYRotation = maxRotationY;
            }
            else
            {
                _currentTargetYRotation = -maxRotationY;
            }
        }

        Quaternion targetRotation = Quaternion.Euler(
            mainCamera.transform.eulerAngles.x,
            _currentTargetYRotation,
            mainCamera.transform.eulerAngles.z
        );

        mainCamera.transform.rotation = Quaternion.RotateTowards(
            mainCamera.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        previousX = currentX;
    }
}
