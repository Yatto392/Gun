
using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float fireCooldown = 1f;
    private float lastFireTime = -1f;
    public float rotationSpeed = 5f;

    private Transform player;

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (player == null) return;

        FacePlayer();

        if (Time.time > lastFireTime + fireCooldown)
        {
            Fire();
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void Fire()
    {
        if (bulletPrefab == null || bulletSpawnPoint == null || player == null)
        {
            Debug.LogError("Bullet Prefab, Spawn Point, or Player not set for the enemy.", this);
            return;
        }

        lastFireTime = Time.time;
        Vector3 directionToPlayer = (player.position - bulletSpawnPoint.position).normalized;
        Quaternion bulletLookRotation = Quaternion.LookRotation(directionToPlayer);

        Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletLookRotation);
    }
}
