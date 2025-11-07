using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTime : LevelCondition
{
    private float m_time;

    private BoardsController m_boardsController;
    private TrayController m_trayController;

    private GameManager m_mngr;

    public override void Setup(float value, BoardsController boardsController, GameManager mngr)
    {
        base.Setup(value, boardsController, mngr);

        m_mngr = mngr;
        m_boardsController = boardsController;
        m_trayController = m_boardsController.m_TrayController;
        m_time = value;

        m_boardsController.OnMoveEvent += OnMove;

        UpdateText();
    }

    private void OnMove()
    {
        if (m_conditionCompleted) return;

        if (m_boardsController.IsAllBoardCleared())
        {
            m_boardsController.IsWin = true;
            OnConditionComplete();
            return;
        }
    }

    private void Update()
    {
        if (m_conditionCompleted) return;

        if (m_mngr.State != GameManager.eStateGame.GAME_STARTED) return;

        m_time -= Time.deltaTime;

        UpdateText();

        if (m_time <= 0)
        {
            m_boardsController.IsWin = false;
            OnConditionComplete();
        }
    }

    protected void UpdateText()
    {
        if (m_time < 0f) return;

        m_txt.text = string.Format("TIME:\n{0:00}", m_time);
    }
}
