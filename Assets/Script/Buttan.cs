using System.Collections.Generic;
using UnityEngine;

public class Buttan : MonoBehaviour
{
    [SerializeField] Material[] materialArray = new Material[2];
    private int count;
    private bool isActivated;

    public List<GameObject> objectsToActivate = new List<GameObject>();
    public List<GameObject> objectsToDeactivate = new List<GameObject>();

    void Start()
    {
        InitializeState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("弾が触れた");
            UpdateMaterial();
            ToggleActivation();
        }
    }

    private void InitializeState()
    {
        count = 0;
        isActivated = false;

        // 初期マテリアル設定
        if (materialArray.Length > 0)
        {
            GetComponent<MeshRenderer>().material = materialArray[count];
        }

        // 初期アクティブ状態設定
        foreach (GameObject obj in objectsToActivate)
        {
            obj.SetActive(isActivated);
        }

        foreach (GameObject obj in objectsToDeactivate)
        {
            obj.SetActive(!isActivated);
        }
    }

    private void UpdateMaterial()
    {
        count = (count + 1) % materialArray.Length;
        GetComponent<MeshRenderer>().material = materialArray[count];
    }

    private void ToggleActivation()
    {
        isActivated = !isActivated; // 状態を反転

        foreach (GameObject obj in objectsToActivate)
        {
            obj.SetActive(isActivated);
        }

        foreach (GameObject obj in objectsToDeactivate)
        {
            obj.SetActive(!isActivated);
        }
    }
}
