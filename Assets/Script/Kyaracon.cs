







using System.Collections;


using System.Collections.Generic;


using TMPro;


using UnityEngine;


using UnityEngine.UI; // UIコンポーネントを使用するために追加





// しゃがみ動作の制御モード


public enum CrouchMode { Toggle, Hold }





public class Kyaracon : MonoBehaviour


{


    public List<GameObject> targetObjects;


    private int currentIndex = 0;


    public GameObject bulletPrefab; // 弾丸のプレハブ





    public float fireCooldown = 0.5f; // 弾丸の発射クールダウン時間


    private float lastFireTime = -1.0f; // 最後に弾丸を発射した時間





    public float moveCooldown = 1.0f; // 移動のクールダウン時間（秒）


    private float lastMoveTime = -1.0f; // 最後に移動した時間


    public Image cooldownGauge; // クールダウン表示用のUI画像





    [Header("Camera Rotation")]


    public Camera mainCamera;


    public float rotationSpeed = 40f; // Degrees per second


    public float maxRotationY = 3.6f;





    private float previousX;


    private float _currentTargetYRotation;


    private float lockedZPosition; // Z座標を固定するための変数


    private float _targetCharacterYRotation; // キャラクターの目標Y軸回転





    [Header("Ammo")]


    public int maxAmmo = 150; // 最大総弾薬数


    public int currentAmmo = 150; // 現在の総弾薬数


    public int magazineSize = 30; // マガジンのサイズ


    public int currentMagazineAmmo; // 現在のマガジン内の弾数


    public float reloadTime = 1.5f; // リロード時間


    private bool isReloading = false; // リロード中か


    public TextMeshProUGUI ammoText; // 弾薬数を表示するUIテキスト





    [Header("Bullet Spawn Point")]


    public Transform bulletSpawnPoint; // 弾丸の発射位置を定義するTransform





    [Header("Crouch Settings")]


    public Animator animator;


    public CrouchMode crouchMode = CrouchMode.Toggle;


    private const string syagamiParameter = "Syagami";


    private const string syagamiIdouParameter = "Syagami_idou";


    public float syagamiMoveAnimationDuration = 0.5f; // しゃがみ移動アニメーションの長さ


    public float postCrouchMoveDelay = 0.15f; // しゃがみ移動アニメーション後の追加待機時間








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





        // Animatorの参照を取得


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


        lockedZPosition = transform.position.z; // 開始時のZ座標を保存


        _targetCharacterYRotation = transform.eulerAngles.y; // キャラクターの初期Y軸回転を保存





        currentMagazineAmmo = magazineSize; // 最初はマガジンを満タンに


        UpdateAmmoUI();


    }





    // Update is called once per frame


    void Update()


    {


        HandleCrouchInput();





        // リロード中は他のアクションを不可


        if (isReloading)


        {


            return;


        }





        // --- 現在の足場が有効かチェック ---


        if (targetObjects != null && targetObjects.Count > 0)


        {


            // currentIndexが範囲外になることを防ぐ


            if (currentIndex >= targetObjects.Count)


            {


                currentIndex = targetObjects.Count - 1;


            }





            GameObject currentPos = targetObjects[currentIndex];


            if (currentPos == null || !currentPos.activeInHierarchy)


            {


                // 足場が消えたら代わりの足場を探して移動


                FindAndMoveToNewPos();


            }


        }


        // --- 足場チェックここまで ---





        // クールダウンゲージを更新


        if (cooldownGauge != null)


        {


            float timeSinceLastMove = Time.time - lastMoveTime;


            cooldownGauge.fillAmount = Mathf.Clamp01(timeSinceLastMove / moveCooldown);


        }





        // Rキーでリロード


        if (Input.GetKeyDown(KeyCode.R) && currentMagazineAmmo < magazineSize && currentAmmo > 0)


        {


            StartCoroutine(Reload());


            return; // リロード中は他の処理をしない


        }








        // スペースキーで通常弾を発射


        if (Input.GetKeyDown(KeyCode.Space))


        {


            Debug.Log("Space key pressed."); // スペースキーが押されたかログ出力





            // クールダウン、マガジン弾数、リロード中でないことをチェック


            bool canFire = Time.time >= lastFireTime + fireCooldown && currentMagazineAmmo > 0 && !isReloading;





            // 発射できない理由をログに出力


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


                // ここで自動リロードを開始することも可能


                // StartCoroutine(Reload());


            }


        }





        if (targetObjects == null || targetObjects.Count == 0)


        {


            return;


        }


        


        // --- 移動入力のデバッグ ---


        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))


        {


             Debug.Log("Move key (A or D) pressed.");


             bool canMoveCheck = Time.time >= lastMoveTime + moveCooldown;


             if (!canMoveCheck)


             {


                 Debug.LogWarning($"Move key pressed, but cannot move due to cooldown!");


                 Debug.Log($"Time: {Time.time}, lastMoveTime: {lastMoveTime}, moveCooldown: {moveCooldown}");


             }


        }


        // --- デバッグここまで ---





        // クールダウンが終了しているかチェック


        bool canMove = Time.time >= lastMoveTime + moveCooldown;





        if (!canMove) return; // クールダウン中なら何もしない





        // Aキーで前のオブジェクトへ移動 (インデックス減少)


        if (Input.GetKeyDown(KeyCode.A))


        {


            int previousIndex = currentIndex - 1;


            if (previousIndex >= 0)


            {


                if (targetObjects[previousIndex] != null && targetObjects[previousIndex].activeInHierarchy)


                {


                    _targetCharacterYRotation = -90f; // キャラクターの目標回転を設定


                    HandleMove(previousIndex, _targetCharacterYRotation);


                }


            }


        }





        // Dキーで次のオブジェクトへ移動 (インデックス増加)


        if (Input.GetKeyDown(KeyCode.D))


        {


            int nextIndex = currentIndex + 1;


            if (nextIndex < targetObjects.Count)


            {


                if (targetObjects[nextIndex] != null && targetObjects[nextIndex].activeInHierarchy)


                {


                    _targetCharacterYRotation = 90f; // キャラクターの目標回転を設定


                    HandleMove(nextIndex, _targetCharacterYRotation);


                }


            }


        }


    }





    private void HandleCrouchInput()


    {


        if (animator == null) return;





        switch (crouchMode)


        {


            case CrouchMode.Hold:


                // Sキーを押し続けている間だけSyagamiをtrueにする


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


                // Sキーを押すたびにSyagamiのtrue/falseを切り替える


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


        if (bulletSpawnPoint == null) // 新しいnullチェック


        {


            Debug.LogError("Bullet Spawn Point is not set in the inspector! Cannot fire bullet.", this);


            return;


        }








        currentMagazineAmmo--; // 弾を1発消費


        lastFireTime = Time.time; // 発射時間を更新


        UpdateAmmoUI();





        // bulletSpawnPointの位置と回転を使用して弾丸を生成


        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);


        BulletController bulletCtrl = bullet.GetComponent<BulletController>();





        if (bulletCtrl != null)


        {


            // ターゲットをセットしないので、まっすぐ飛ぶ


            bulletCtrl.Target = null;


        }


    }





    IEnumerator Reload()


    {


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


    }





    private void HandleMove(int newIndex, float targetRotation)


    {


        GameObject destination = targetObjects[newIndex];


        GameObject currentPos = targetObjects[currentIndex];





        // 移動先のタグが "Syagami" または現在の足場のタグが "Syagami" かチェック


        if (destination.CompareTag("Syagami") || currentPos.CompareTag("Syagami"))


        {


            // しゃがみ状態かチェック


            bool isCrouching = animator.GetBool(syagamiParameter);


            if (isCrouching)


            {


                // キャラクターが正しい方向を向いているかチェック (誤差1度を許容)


                float currentYAngle = transform.eulerAngles.y;


                if (Mathf.Abs(Mathf.DeltaAngle(currentYAngle, targetRotation)) < 1.0f)


                {


                    // 正しい方向を向いていれば、アニメーション付きで移動


                    StartCoroutine(MoveWithAnimation(newIndex));


                }


                else


                {


                    // 正しい方向を向いていなければ、今回は回転のみ。移動は再度キーを押す必要がある。


                    Debug.Log("Not facing the correct direction for Syagami move. Rotating first.");


                }


            }


            else


            {


                // しゃがんでいない場合は移動しない


                Debug.Log("Cannot move from/to Syagami point, not crouching.");


            }


        }


        else


        {


            // どちらも "Syagami" タグが付いていなければ通常通り移動


            MoveTo(newIndex);


        }


    }





    IEnumerator MoveWithAnimation(int newIndex)


    {


        // クールダウンを開始し、他の移動を防ぐ


        lastMoveTime = Time.time;


        animator.SetTrigger(syagamiIdouParameter);





        // アニメーションが終わるまで待つ


        yield return new WaitForSeconds(syagamiMoveAnimationDuration);





        // コンマ数秒待機


        yield return new WaitForSeconds(postCrouchMoveDelay);





        // クールダウンを再更新せずに移動


        MoveTo(newIndex, false);


        animator.SetBool(syagamiParameter, false); // しゃがみ状態を解除


    }








    private void MoveTo(int newIndex, bool updateCooldown = true)


    {


        currentIndex = newIndex;


        Vector3 newPosition = transform.position;


        newPosition.x = targetObjects[currentIndex].transform.position.x;


        transform.position = newPosition;





        if (updateCooldown)


        {


            lastMoveTime = Time.time; // 移動時間を更新


        }


    }





    private void FindAndMoveToNewPos()


    {


        // まず左を探す (リストの範囲内かつ、オブジェクトが存在しアクティブか)


        int previousIndex = currentIndex - 1;


        if (previousIndex >= 0 && targetObjects[previousIndex] != null && targetObjects[previousIndex].activeInHierarchy)


        {


            MoveTo(previousIndex);


            Debug.Log($"Moved left to index {previousIndex} as current position became inactive (preferred).");


            return;


        }





        // 次に右を探す (リストの範囲内かつ、オブジェクトが存在しアクティブか)


        int nextIndex = currentIndex + 1;


        if (nextIndex < targetObjects.Count && targetObjects[nextIndex] != null && targetObjects[nextIndex].activeInHierarchy)


        {


            MoveTo(nextIndex);


            Debug.Log($"Moved right to index {nextIndex} as current position became inactive.");


            return;


        }





        // 移動先がなかった場合


        Debug.LogWarning("Could not find an active position to move to!");


    }








    void LateUpdate()


    {


        // --- Z座標の固定 ---


        Vector3 pos = transform.position;


        if (pos.z != lockedZPosition)


        {


            pos.z = lockedZPosition;


            transform.position = pos;


        }





        // --- キャラクターのY軸回転 ---


        Quaternion currentCharacterRotation = transform.rotation;


        Quaternion targetCharacterRotation = Quaternion.Euler(currentCharacterRotation.eulerAngles.x, _targetCharacterYRotation, currentCharacterRotation.eulerAngles.z);


        transform.rotation = Quaternion.RotateTowards(currentCharacterRotation, targetCharacterRotation, rotationSpeed * Time.deltaTime);


        


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



