using Unity.VisualScripting;
using UnityEngine;

public class HPsystem : MonoBehaviour
{
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is creat

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Bullet")
        {
            animator.SetTrigger("Damage");
        }
    }
}
