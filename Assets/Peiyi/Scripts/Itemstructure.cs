using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int price;
    public Vector2Int size; //item width*hieght
    public Sprite icon; // it is for the shop
    public Color itemColor=Color.white;

    [System.Serializable]
    public class RowData
    {
        public List<int> row = new List<int>(); // store row data
    }

    [SerializeField]
    public List<RowData> shapeMatrix = new List<RowData>(); // pack `List<List<int>>`

    public int[,] GetShapeMatrix()
    {
        if (shapeMatrix.Count == 0) return null; // prevent null row
        int rows = shapeMatrix.Count;
        int cols = shapeMatrix[0].row.Count;
        int[,] matrix = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = shapeMatrix[i].row[j]; // read in data of such block
            }
        }
        return matrix;
    }
}


