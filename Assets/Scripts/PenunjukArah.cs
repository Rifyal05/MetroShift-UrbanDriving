using UnityEngine;

public class PenunjukArah : MonoBehaviour
{
    public Transform targetObjektif;
    public float kecepatanPutar = 10f;

    private Transform objTransform;

    private void Start()
    {
        objTransform = transform; // Caching transform
    }

    private void Update()
    {
        if (targetObjektif != null)
        {
            Vector3 arah = targetObjektif.position - objTransform.position;
            arah.y = 0f; 

            if (arah.sqrMagnitude > 0.01f) 
            {
                Quaternion targetRotasi = Quaternion.LookRotation(arah);
                objTransform.rotation = Quaternion.Slerp(objTransform.rotation, targetRotasi, Time.deltaTime * kecepatanPutar);
            }
        }
    }
}