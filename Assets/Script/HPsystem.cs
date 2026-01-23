
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI; // Required for Image
using TMPro;
using System.Collections;

public class HPsystem : MonoBehaviour
{
    public GameObject pos;
    public int Hp;
    public int maxHp = 100; // Added Max HP
     [SerializeField]
    private GameObject DamageObj;
    [SerializeField]
    private GameObject PosObj;
    [SerializeField]
    private Vector3 AdjPos;
    public Animator animator;

    [Header("UI References")] // New header for UI elements
    public Image hpFillImage; // Reference to the Image component for HP fill

    
    void Start()
    {
        pos.SetActive(false);
        Hp = maxHp; // Initialize current HP to max HP
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = 1f; // Start with full HP
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Bullet"))
        {
            BulletState bulletState = other.gameObject.GetComponent<BulletState>();
            if (bulletState != null)
            {
                animator.SetTrigger("Damage");
                ViewDamage(bulletState.Damagep);
            }
        }
    }

    private void ViewDamage(int _damage)
    {
        GameObject _damageObj = Instantiate(DamageObj);
        _damageObj.GetComponent<TextMeshPro>().text = _damage.ToString();
        Vector3 damagePosition = PosObj.transform.position + AdjPos;
        damagePosition.x += Random.Range(-0.5f, 0.5f); // Add random X offset
        _damageObj.transform.position = damagePosition;
        
        Hp -= _damage;
        Hp = Mathf.Max(0, Hp); // Ensure HP doesn't go below 0

        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = (float)Hp / maxHp; // Update HP fill UI
        }

        if (Hp <= 0) // Changed from < 0 || == 0 to just <= 0 for clarity
        {
            pos.SetActive(true);
            Destroy(gameObject);
        }
    }
}