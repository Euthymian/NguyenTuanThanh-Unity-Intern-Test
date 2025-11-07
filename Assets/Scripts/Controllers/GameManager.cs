using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action OnActivateAutoWinEvent = delegate { };
    public event Action OnActivateAutoLoseEvent = delegate { };

    public event Action<eStateGame> StateChangedAction = delegate { };

    public void ActivateAutoWin()
    {
        OnActivateAutoWinEvent();
    }

    public void ActivateAutoLose()
    {
        OnActivateAutoLoseEvent();
    }

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        LOSE,
        WIN
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardsController m_boardController;
    public BoardsController BoardController => m_boardController;

    public UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    void Update()
    {
        
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardsController>();
        m_boardController.StartGame(this, m_gameSettings, mode);

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_boardController, this);
        }
        else if(mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelTime, m_boardController, this);
        }

        m_levelCondition.ConditionCompleteEvent += GameFinished;

        State = eStateGame.GAME_STARTED;
    }

    public void GameFinished()
    {
        StartCoroutine(WaitBoardController(m_boardController.IsWin));
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController(bool win)
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);

        State = win ? eStateGame.WIN : eStateGame.LOSE;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameFinished;

            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }
}
