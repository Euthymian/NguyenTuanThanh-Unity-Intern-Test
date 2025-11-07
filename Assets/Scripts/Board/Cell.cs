using DG.Tweening;
using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private Board m_board;
    private int m_row, m_col;
    private eCellType m_type;
    private SpriteRenderer m_visual;
    private Animator m_anim;

    public int CellTypeInt => (int)m_type;
    public eCellType CellType => m_type;
    public bool CanBeSelected { get; set; } = true;
    public bool OnTray { get; set; } = false;

    public bool CanAddBackFromTrayToBoard => m_board.m_boardsController.LevelMode == GameManager.eLevelMode.MOVES ? false : true;

    public int Row => m_row;
    public int Col => m_col;
    public Board Board => m_board;

    // Track how many cells are blocking this cell
    private int m_blockingCount = 0; // -> max 4

    public void SetType(eCellType type)
    {
        m_type = type;
    }

    public void Setup(Board board, int row, int col)
    {
        m_board = board;
        m_row = row;
        m_col = col;

        string pathToVisual = "";
        switch (m_type)
        {
            case eCellType.TYPE_ONE: pathToVisual = Constants.PREFAB_NORMAL_TYPE_ONE; break;
            case eCellType.TYPE_TWO: pathToVisual = Constants.PREFAB_NORMAL_TYPE_TWO; break;
            case eCellType.TYPE_THREE: pathToVisual = Constants.PREFAB_NORMAL_TYPE_THREE; break;
            case eCellType.TYPE_FOUR: pathToVisual = Constants.PREFAB_NORMAL_TYPE_FOUR; break;
            case eCellType.TYPE_FIVE: pathToVisual = Constants.PREFAB_NORMAL_TYPE_FIVE; break;
            case eCellType.TYPE_SIX: pathToVisual = Constants.PREFAB_NORMAL_TYPE_SIX; break;
            case eCellType.TYPE_SEVEN: pathToVisual = Constants.PREFAB_NORMAL_TYPE_SEVEN; break;
        }

        GameObject go = Instantiate(Resources.Load<GameObject>(pathToVisual));
        m_visual = go.GetComponent<SpriteRenderer>();
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        transform.position = new Vector3(transform.position.x, transform.position.y, -m_board.BoardIndex);

        go.GetComponent<SpriteRenderer>().sortingOrder = board.BoardIndex;

        m_anim = GetComponent<Animator>();
    }

    public void PlayExplodeAnimation()
    {
        m_anim.Play("explode");
    }

    public void InitializeBlockingStatus()
    {
        m_blockingCount = m_board.CountBlockingCells(m_row, m_col);
        UpdateVisualState();
    }

    public void OnBlockingCellRemoved()
    {
        m_blockingCount--;
        if (m_blockingCount < 0) m_blockingCount = 0;
        UpdateVisualState();
    }

    public void OnBlockingCellAdded()
    {
        m_blockingCount++;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (m_blockingCount == 0)
            ActivateCell();
        else
            DeactivateCell();
    }

    public void ActivateCell()
    {
        CanBeSelected = true;
        if (m_visual != null)
        {
            m_visual.color = new Color(m_visual.color.r, m_visual.color.g, m_visual.color.b, 1f);
        }
    }

    public void DeactivateCell()
    {
        CanBeSelected = false;
        if (m_visual != null)
        {
            m_visual.color = new Color(m_visual.color.r, m_visual.color.g, m_visual.color.b, 0.4f);
        }
    }

    public void Destroy()
    {
        m_board.RemoveCell(m_row, m_col, false);
        Destroy(gameObject, 0.3f);
    }
}