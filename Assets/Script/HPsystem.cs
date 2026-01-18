using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HPsystem : MonoBehaviour
{
     [SerializeField]
    private GameObject DamageObj;
    [SerializeField]
    private GameObject PosObj;
    [SerializeField]
    private Vector3 AdjPos;
    public Animator animator;
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
        _damageObj.GetComponent<TextMesh>().text = _damage.ToString();
        _damageObj.transform.position = PosObj.transform.position + AdjPos;
    }
}
