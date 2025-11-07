using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private int m_boardIndex;
    public BoardsController m_boardsController;
    private int m_size;
    private Cell[,] m_cells;
    private Transform m_root;

    public int BoardIndex => m_boardIndex;
    public Cell[,] Cells => m_cells;
    public int Size => m_size;

    public Board(Transform transform, int size, List<int> typeOfEachCellList, int startIndex, int boardIndex, BoardsController boardsController)
    {
        m_boardsController = boardsController;
        m_size = size;
        m_root = transform;
        m_cells = new Cell[m_size, m_size];
        m_boardIndex = boardIndex;

        CreateBoard(typeOfEachCellList, startIndex);
    }

    public void Clear()
    {
        foreach(Cell c in m_cells)
        {
            if(c != null)
            {
                c.Destroy();
            }
        }
    }

    private void CreateBoard(List<int> typeOfEachCellList, int startIndex)
    {
        Vector3 origin = new Vector3(-m_size * 0.5f + 0.5f, -m_size * 0.5f + 0.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int x = 0; x < m_size; x++)
        {
            for (int y = 0; y < m_size; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.name = $"b{m_boardIndex}-{x}-{y}";
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                int typeIndex = typeOfEachCellList[startIndex];

                switch (typeIndex)
                {
                    case 0: cell.SetType(eCellType.TYPE_ONE); break;
                    case 1: cell.SetType(eCellType.TYPE_TWO); break;
                    case 2: cell.SetType(eCellType.TYPE_THREE); break;
                    case 3: cell.SetType(eCellType.TYPE_FOUR); break;
                    case 4: cell.SetType(eCellType.TYPE_FIVE); break;
                    case 5: cell.SetType(eCellType.TYPE_SIX); break;
                    case 6: cell.SetType(eCellType.TYPE_SEVEN); break;
                }

                cell.Setup(this, x, y);
                m_cells[x, y] = cell;
                startIndex++;
            }
        }
    }

    public Cell GetCellAt(int row, int col)
    {
        if (row < 0 || row >= m_size || col < 0 || col >= m_size)
            return null;

        return m_cells[row, col];
    }

    public int CountBlockingCells(int row, int col)
    {
        return m_boardsController.CountBlockingCellsAt(m_boardIndex, row, col);
    }

    public void RemoveCell(int row, int col, bool notify)
    {
        if (row >= 0 && row < m_size && col >= 0 && col < m_size)
        {
            m_cells[row, col] = null;

            // Not notify if cell exploded from tray
            if(notify)
                m_boardsController.NotifyAffectedCellsOnRemove(m_boardIndex, row, col);
        }
    }

    public void AddCellBack(Cell cell, int row, int col)
    {
        if (row >= 0 && row < m_size && col >= 0 && col < m_size)
        {
            m_cells[row, col] = cell;

            cell.InitializeBlockingStatus();
            m_boardsController.UpdateCurrentAvailableCellList(cell);

            m_boardsController.NotifyAffectedCellsOnAdd(m_boardIndex, row, col);
        }
    }

    
}