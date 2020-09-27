using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BreakToPiece))]
public class PiecesCompoenet : MonoBehaviour
{
    BreakToPiece breakTo;
    Rigidbody2D m2Dphysic;
    public float breakForce=20;
    public float destoryTime = 15;
    private void Start()
    {
        breakTo = gameObject.GetComponent<BreakToPiece>();
        m2Dphysic = gameObject.GetComponent<Rigidbody2D>();

        breakTo.eCreateNewPiece += GiveComponentTo;

    }
    private void OnDestroy()
    {
        breakTo.eCreateNewPiece -= GiveComponentTo;
    }
    void GiveComponentTo(GameObject obj)
    {
        //給元件
        if (m2Dphysic != null)
        {
            //複製
            Rigidbody2D tmp = obj.AddComponent<Rigidbody2D>(m2Dphysic);
            //tmp.gameObject.AddComponent<BoxCollider2D>();

            tmp.velocity=(new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * breakForce);
        }

        //設定摧毀時間
        Destroy(obj, destoryTime);
    }
}
