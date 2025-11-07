using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCondition : MonoBehaviour
{
    public event Action ConditionCompleteEvent = delegate { };

    protected Text m_txt;

    protected bool m_conditionCompleted = false;

    public virtual void Setup(float value, BoardsController boardsController, GameManager mngr)
    {
        m_txt = mngr.m_uiMenu.GetLevelConditionView();
    }

    public virtual void Setup(BoardsController boardsController, GameManager mngr)
    {

        m_txt = mngr.m_uiMenu.GetLevelConditionView();
    }

    protected void OnConditionComplete()
    {
        m_conditionCompleted = true;

        ConditionCompleteEvent();
    }

    protected virtual void OnDestroy()
    {

    }
}
