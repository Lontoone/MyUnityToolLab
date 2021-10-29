using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBaseManager : MonoBehaviour
{
    public float cellSize;
    public float spacing;
    public Transform originPoint;
    public Vector2Int gridSize;
    public List<Cell> cells;

    [ContextMenu("GenerateGrid")]
    public void GenerateGrid()
    {
        cells.Clear();

        //從原點，間隔生成
        Vector3 _pos = originPoint.position;
        float halfCellSize = cellSize * 0.5f;
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                Cell _cell = new Cell();
                _cell.index = i * gridSize.y + j;
                _cell.gridPosition = new Vector2Int(i, j);

                _cell.worldPos = _pos + new Vector3(i * halfCellSize + spacing, 0, j * halfCellSize + spacing);
                cells.Add(_cell);
            }
        }

    }

    public void GetCellPos(Vector2Int pos) {
        
    }
    public void GetCellPos(int cellIndex) { }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            Gizmos.DrawWireSphere(cells[i].worldPos, 0.2f);
        }
    }
}

[System.Serializable]
public class Cell
{
    public int index;
    public Vector2Int gridPosition;
    public Vector3 worldPos;
}