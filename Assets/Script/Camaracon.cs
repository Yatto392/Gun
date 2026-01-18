
using UnityEngine;

public class Camaracon : MonoBehaviour
{
    public Transform player;
    public float smoothTime = 0.3f;
    public float offsetX = 0f;

    private float xVelocity = 0.0f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player object not found. Make sure your player has the 'Player' tag.");
                this.enabled = false;
                return;
            }
        }

        // If offsetX is 0 (default), calculate an initial offset based on current positions
        if (offsetX == 0f) {
            offsetX = transform.position.x - player.position.x;
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // --- Position Follow Logic ---
            float targetX = player.position.x + offsetX;
            float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref xVelocity, smoothTime);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }
}
