using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMoves : LevelCondition
{
    private BoardsController m_boardsController;
    private TrayController m_trayController;

    public override void Setup(BoardsController board, GameManager m)
    {
        base.Setup(board, m);

        m_boardsController = board;

        m_trayController = m_boardsController.m_TrayController;

        m_boardsController.OnMoveEvent += OnMove;

        m_txt.text = "CLEAR";
    }

    private void OnMove()
    {
        StartCoroutine(WaitForAnyAnimationFinish());
    }

    IEnumerator WaitForAnyAnimationFinish()
    {
        yield return new WaitForSeconds(1f);

        if (m_conditionCompleted) yield break;

        if (m_boardsController.IsAllBoardCleared())
        {
            m_boardsController.IsWin = true;
            OnConditionComplete();
            yield break;
        }

        // Tray controller
        if (m_trayController.IsFull)
        {
            m_boardsController.IsWin = false;
            OnConditionComplete();
            yield break;
        }
    }

    protected override void OnDestroy()
    {
        if (m_boardsController != null) m_boardsController.OnMoveEvent -= OnMove;

        base.OnDestroy();
    }
}
