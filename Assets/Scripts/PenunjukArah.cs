using UnityEngine;

public class PenunjukArah : MonoBehaviour
{
    public Transform targetObjektif;
    public float kecepatanPutar = 10f;

    private void Update()
    {
        if (targetObjektif != null)
        {
            Vector3 arah = targetObjektif.position - transform.position;
            arah.y = 0f; 

            if (arah != Vector3.zero)
            {
                Quaternion targetRotasi = Quaternion.LookRotation(arah);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotasi, Time.deltaTime * kecepatanPutar);
            }
        }
    }
}