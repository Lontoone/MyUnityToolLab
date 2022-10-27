using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class JiugonggeCell : MonoBehaviour
{
    JiugonggeGrid grid;
    [Header("Auto asign")]
    public int index;
    [Header("Correct index as answor")]
    public int ans;


    SpriteRenderer sp;
    public float radious
    {
        get { return (sp.bounds.extents.x); }
    }

    public void Start()
    {
        sp = FindObjectOfType<SpriteRenderer>();
        grid = FindObjectOfType<JiugonggeGrid>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        JiugonggeCell _cell = collision.transform.GetComponent<JiugonggeCell>();
        //Debug.Log(collision.gameObject.name);

        //和空白方塊互換位置
        if (_cell != null && _cell == grid.blankCell)
        {
            grid.SwapCell(this, _cell);
            //_cell.transform.position = grid.originPos[_cell.index];

        }
    }





}
