using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrayController : MonoBehaviour
{
    private List<Cell> m_cellsOnTray = new List<Cell>();
    public List<Cell> CellsOnTray => m_cellsOnTray;
    private BoardsController m_boardsController;

    [SerializeField] private float m_moveDuration = 0.3f;

    public bool IsFull => m_cellsOnTray.Count >= 5;

    // Store cell info for returning to board
    private class CellInfo
    {
        public Cell cell;
        public Board board;
        public int row;
        public int col;
        public Vector3 originalPosition;
    }
    private List<CellInfo> m_cellInfos = new List<CellInfo>();

    public void Initialize(BoardsController boardsController)
    {
        m_boardsController = boardsController;
    }

    public void AddCell(Cell cell)
    {
        if (IsFull) return;

        // Store cell info before moving
        CellInfo info = new CellInfo
        {
            cell = cell,
            board = cell.Board,
            row = cell.Row,
            col = cell.Col,
            originalPosition = cell.transform.position
        };
        m_cellInfos.Add(info);

        m_cellsOnTray.Add(cell);
        cell.transform.parent = this.transform;

        cell.Board.RemoveCell(cell.Row, cell.Col, true);
        cell.OnTray = true;
        cell.CanBeSelected = false;

        SortTray();
        StartCoroutine(WaitToMoveBeforeCheckExplode());
    }

    IEnumerator WaitToMoveBeforeCheckExplode()
    {
        yield return new WaitForSeconds(m_moveDuration);
        CheckValidToExplode();
    }

    public void SortTray()
    {
        // Sort both lists in sync
        // Number of cells is small, so using simple bubble sort for clarity
        for (int i = 0; i < m_cellsOnTray.Count - 1; i++)
        {
            for (int j = i + 1; j < m_cellsOnTray.Count; j++)
            {
                if (m_cellsOnTray[i].CellTypeInt > m_cellsOnTray[j].CellTypeInt)
                {
                    // Swap cells
                    Cell tempCell = m_cellsOnTray[i];
                    m_cellsOnTray[i] = m_cellsOnTray[j];
                    m_cellsOnTray[j] = tempCell;

                    // Swap infos
                    CellInfo tempInfo = m_cellInfos[i];
                    m_cellInfos[i] = m_cellInfos[j];
                    m_cellInfos[j] = tempInfo;
                }
            }
        }

        UpdateVisual();
    }

    public void CheckValidToExplode()
    {
        if (m_cellsOnTray.Count < 3)
            return;

        for (int i = 1; i < m_cellsOnTray.Count - 1; i++)
        {
            if (m_cellsOnTray[i].CellTypeInt == m_cellsOnTray[i - 1].CellTypeInt &&
                m_cellsOnTray[i].CellTypeInt == m_cellsOnTray[i + 1].CellTypeInt)
            {
                for (int j = i + 1; j >= i - 1; j--)
                {
                    m_cellsOnTray[j].PlayExplodeAnimation();
                    m_cellsOnTray[j].Destroy();
                    m_cellsOnTray.RemoveAt(j);
                    m_cellInfos.RemoveAt(j);
                }

                break;
            }
        }

        StartCoroutine(WaitFinishExplodeAnim());
    }

    IEnumerator WaitFinishExplodeAnim()
    {
        yield return new WaitForSeconds(m_moveDuration);
        SortTray();
    }

    public void ReturnCellToBoard(int index)
    {
        if (index < 0 || index >= m_cellsOnTray.Count) return;

        Cell cell = m_cellsOnTray[index];
        CellInfo info = m_cellInfos[index];

        // Remove from tray
        m_cellsOnTray.RemoveAt(index);
        m_cellInfos.RemoveAt(index);

        // Return to board
        cell.transform.DOKill();
        cell.transform.DOMove(info.originalPosition, m_moveDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(cell.gameObject)
            .OnComplete(() =>
            {
                cell.transform.SetParent(m_boardsController.transform, true);
            });

        // Re-add to board
        info.board.AddCellBack(cell, info.row, info.col);
        cell.OnTray = false;

        UpdateVisual();
    }

    public void ReturnCellToBoard(Cell cell)
    {
        int index = m_cellsOnTray.IndexOf(cell);
        ReturnCellToBoard(index);
    }

    public void UpdateVisual()
    {
        //for (int i = 0; i < m_cellsOnTray.Count; i++)
        //{
        //    Vector3 targetPos = new Vector3(i - 2, 0, 0);
        //    m_cellsOnTray[i].transform.localPosition = targetPos;
        //}

        for (int i = 0; i < m_cellsOnTray.Count; i++)
        {
            Vector3 targetPos = new Vector3(i - 2, 0f, 0f);
            var t = m_cellsOnTray[i].transform;

            t.DOKill();
            t.DOLocalMove(targetPos, m_moveDuration)
             .SetEase(Ease.OutQuad)
             .SetLink(t.gameObject);
        }
    }

    internal void Clear()
    {
        m_cellsOnTray.Clear();
        m_cellInfos.Clear();
        Destroy(gameObject);
    }
}