using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardsController : MonoBehaviour
{

    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private List<Board> m_boardList = new List<Board>();
    private GameManager m_gameManager;
    private Camera m_cam;
    private GameSettings m_gameSettings;
    private bool m_gameFinished;
    public TrayController m_TrayController;

    private Coroutine m_autoCo;

    public GameManager.eLevelMode LevelMode { get; private set; }

    private List<Cell> m_currentAvailableCellList = new List<Cell>();

    public void StartGame(GameManager gameManager, GameSettings gameSettings, GameManager.eLevelMode mode)
    {
        LevelMode = mode;

        m_TrayController = Instantiate(Resources.Load<GameObject>(Constants.PREFAB_TRAY_CONTROLLER)).GetComponent<TrayController>();
        m_TrayController.Initialize(this);

        m_gameManager = gameManager;
        m_gameManager.StateChangedAction += OnGameStateChange;
        m_gameManager.OnActivateAutoWinEvent += GameManager_OnActivateAutoWinEvent;
        m_gameManager.OnActivateAutoLoseEvent += GameManager_OnActivateAutoLoseEvent;

        m_gameSettings = gameSettings;
        m_cam = Camera.main;

        // Calculate total cells
        int totalCells = 0;
        for (int i = 0; i < gameSettings.Height; i++)
        {
            totalCells += (gameSettings.BaseSize - i) * (gameSettings.BaseSize - i);
        }

        if (totalCells % 3 != 0)
        {
            Debug.LogError("Total cells must be multiple of 3");
            Application.Quit();
        }

        // Distribute cell types
        int totalGroup = totalCells / 3;
        int numberOfTypes = Enum.GetValues(typeof(eCellType)).Length;
        int numberOfgroupsEachType = totalGroup / numberOfTypes;
        int remainderGroups = totalGroup % numberOfTypes;

        List<int> typeOfEachCellList = new List<int>();
        for (int i = 0; i < numberOfTypes; i++)
        {
            int cache = numberOfgroupsEachType;
            while (UnityEngine.Random.Range(0, 2) == 1 && remainderGroups > 0)
            {
                cache++;
                remainderGroups--;
            }
            if (i == numberOfTypes - 1 && remainderGroups != 0)
                cache += remainderGroups;

            int numOfElementOfThisType = cache * 3;
            for (int j = 0; j < numOfElementOfThisType; j++)
            {
                typeOfEachCellList.Add(i);
            }
        }

        // Shuffle
        for (int i = 0; i < typeOfEachCellList.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, typeOfEachCellList.Count);
            int temp = typeOfEachCellList[i];
            typeOfEachCellList[i] = typeOfEachCellList[randIndex];
            typeOfEachCellList[randIndex] = temp;
        }

        // Create boards
        int startIndex = 0;
        for (int i = 0; i < gameSettings.Height; i++)
        {
            int boardSize = gameSettings.BaseSize - i;
            m_boardList.Add(new Board(transform, boardSize, typeOfEachCellList, startIndex, i, this));
            startIndex += boardSize * boardSize;
        }

        // Initialize all cell blocking statuses
        InitializeAllCellStatuses();
    }

    private void GameManager_OnActivateAutoLoseEvent()
    {
        if (m_autoCo != null) return;
        m_autoCo = StartCoroutine(AutoLoseCo());
    }

    private void GameManager_OnActivateAutoWinEvent()
    {
        if (m_autoCo != null) return;
        m_autoCo = StartCoroutine(AutoWinCo());
    }

    IEnumerator AutoLoseCo()
    {
        while (m_TrayController.CellsOnTray.Count < 5)
        {
            if (m_currentAvailableCellList.Count > 0)
            {
                // Add cell which has lowest appearance count on current available list
                if (m_TrayController.CellsOnTray.Count == 0)
                {
                    Dictionary<eCellType, int> typeCountDict = new Dictionary<eCellType, int>();
                    foreach (var cell in m_currentAvailableCellList)
                    {
                        if (typeCountDict.ContainsKey((eCellType)cell.CellTypeInt))
                        {
                            typeCountDict[(eCellType)cell.CellTypeInt]++;
                        }
                        else
                        {
                            typeCountDict[(eCellType)cell.CellTypeInt] = 1;
                        }
                    }
                    eCellType minType = eCellType.TYPE_ONE;
                    int minCount = int.MaxValue;
                    foreach (var kvp in typeCountDict)
                    {
                        if (kvp.Value < minCount)
                        {
                            minCount = kvp.Value;
                            minType = kvp.Key;
                        }
                    }
                    foreach (var cell in m_currentAvailableCellList)
                    {
                        if ((eCellType)cell.CellTypeInt != minType)
                        {
                            AddCellToTray(cell);
                            break;
                        }
                    }
                }
                // Add cell which hasnt appeared or has lowest appearance count on tray
                else
                {
                    bool added = false;
                    List<eCellType> typesOnTray = new List<eCellType>();
                    foreach (var e in m_TrayController.CellsOnTray)
                    {
                        if (!typesOnTray.Contains(e.CellType)) typesOnTray.Add(e.CellType);
                    }
                    foreach (var e in m_currentAvailableCellList)
                    {
                        if (!typesOnTray.Contains(e.CellType))
                        {
                            added = true;
                            AddCellToTray(e);
                            break;
                        }
                    }

                    if (!added)
                    {
                        Dictionary<eCellType, int> typeCountDict = new Dictionary<eCellType, int>();
                        foreach (var cell in m_TrayController.CellsOnTray)
                        {
                            if (typeCountDict.ContainsKey((eCellType)cell.CellTypeInt))
                            {
                                typeCountDict[(eCellType)cell.CellTypeInt]++;
                            }
                            else
                            {
                                typeCountDict[(eCellType)cell.CellTypeInt] = 1;
                            }
                        }
                        eCellType minType = eCellType.TYPE_ONE;
                        int minCount = int.MaxValue;
                        foreach (var kvp in typeCountDict)
                        {
                            if (kvp.Value < minCount)
                            {
                                minCount = kvp.Value;
                                minType = kvp.Key;
                            }
                        }
                        foreach (var cell in m_currentAvailableCellList)
                        {
                            if ((eCellType)cell.CellTypeInt != minType)
                            {
                                added = true;
                                AddCellToTray(cell);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                yield break;
            }

            yield return new WaitForSeconds(m_gameSettings.autoDelayTime);
        }
    }

    IEnumerator Add3Cells(eCellType type)
    {
        for(int i=0;i<3;i++)
        {
            foreach(var cell in m_currentAvailableCellList)
            {
                if(cell.CellType == type)
                {
                    AddCellToTray(cell);
                    break;
                }
            }
            yield return new WaitForSeconds(m_gameSettings.autoDelayTime);
        }
    }

    IEnumerator AutoWinCo()
    {
        while (!IsWin)
        {
            if (m_currentAvailableCellList.Count > 0)
            {
                // Add cell which has highest appearance count on current available list
                if (m_TrayController.CellsOnTray.Count == 0)
                {
                    AddCellHasHighestAppearanceOnCurrentAvailableList();
                }
                else
                {
                    bool added = false;

                    // Available cells has 3 cells same type and tray has less than 2 cells -> add those cells
                    Dictionary<eCellType, int> typeCountDictOnAvailableList = new Dictionary<eCellType, int>();
                    foreach (var cell in m_currentAvailableCellList)
                    {
                        if (typeCountDictOnAvailableList.ContainsKey((eCellType)cell.CellTypeInt))
                        {
                            typeCountDictOnAvailableList[(eCellType)cell.CellTypeInt]++;
                        }
                        else
                        {
                            typeCountDictOnAvailableList[(eCellType)cell.CellTypeInt] = 1;
                        }
                    }

                    Dictionary<eCellType, int> typeCountDictOnTray = new Dictionary<eCellType, int>();
                    foreach (var cell in m_TrayController.CellsOnTray)
                    {
                        if (typeCountDictOnTray.ContainsKey((eCellType)cell.CellTypeInt))
                        {
                            typeCountDictOnTray[(eCellType)cell.CellTypeInt]++;
                        }
                        else
                        {
                            typeCountDictOnTray[(eCellType)cell.CellTypeInt] = 1;
                        }
                    }

                    foreach (var kvp in typeCountDictOnAvailableList)
                    {
                        if (kvp.Value >= 3 && m_TrayController.CellsOnTray.Count < 3)
                        {
                            yield return Add3Cells(kvp.Key);
                            added = true;
                        }
                        if (added) break;
                    }

                    if (!added)
                    {
                        // If there is already a type with 2 on tray, add available cell has that type or remove blocking status of affected cells
                        bool hasTypeWith2OnTray = false;
                        eCellType typeAlreadyHas2OnTray = eCellType.TYPE_ONE;
                        foreach (var kvp in typeCountDictOnTray)
                        {
                            if (kvp.Value == 2)
                            {
                                hasTypeWith2OnTray = true;
                                typeAlreadyHas2OnTray = kvp.Key;
                                break;
                            }
                        }
                        if (hasTypeWith2OnTray)
                        {
                            foreach (var cell in m_currentAvailableCellList)
                            {
                                if ((eCellType)cell.CellTypeInt == typeAlreadyHas2OnTray)
                                {
                                    added = true;
                                    AddCellToTray(cell);
                                    break;
                                }
                            }

                            if (!added)
                            {
                                bool foundCandidate = false;
                                foreach (var cell in m_currentAvailableCellList)
                                {
                                    int row = cell.Row;
                                    int col = cell.Col;
                                    Board behindBoard = m_boardList[cell.Board.BoardIndex - 1];

                                    int[] affectedRows = { row, row + 1 };
                                    int[] affectedCols = { col, col + 1 };

                                    foreach (int r in affectedRows)
                                    {
                                        foreach (int c in affectedCols)
                                        {
                                            Cell affectedCell = behindBoard.GetCellAt(r, c);
                                            if (affectedCell != null)
                                            {
                                                if (affectedCell.CellType == typeAlreadyHas2OnTray)
                                                {
                                                    foundCandidate = true;
                                                    added = true;
                                                    AddCellToTray(cell);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (foundCandidate) break;
                                }
                            }
                        }


                        // Add cell which has highest appearance count on tray and on available list
                        if (!added)
                        {
                            eCellType maxType = eCellType.TYPE_ONE;
                            int maxCount = 0;
                            foreach (var kvp in typeCountDictOnTray)
                            {
                                if (kvp.Value > maxCount)
                                {
                                    maxCount = kvp.Value;
                                    maxType = kvp.Key;
                                }
                            }
                            List<eCellType> allTypesHaveSameMaxCount = new List<eCellType>();
                            foreach (var kvp in typeCountDictOnTray)
                            {
                                if (kvp.Value == maxCount)
                                {
                                    allTypesHaveSameMaxCount.Add(kvp.Key);
                                }
                            }
                            eCellType typeHasHighestAppearanceOnAvailableList = allTypesHaveSameMaxCount[0];
                            int highestCountOnAvailableList = 0;
                            foreach (var type in allTypesHaveSameMaxCount)
                            {
                                if (typeCountDictOnAvailableList.ContainsKey(type))
                                {
                                    if (typeCountDictOnAvailableList[type] > highestCountOnAvailableList)
                                    {
                                        highestCountOnAvailableList = typeCountDictOnAvailableList[type];
                                        typeHasHighestAppearanceOnAvailableList = type;
                                    }
                                }
                            }
                            foreach (var cell in m_currentAvailableCellList)
                            {
                                if ((eCellType)cell.CellTypeInt == typeHasHighestAppearanceOnAvailableList)
                                {
                                    added = true;
                                    AddCellToTray(cell);
                                    break;
                                }
                            }
                        }
                    }

                    if (!added)
                    {
                        AddCellHasHighestAppearanceOnCurrentAvailableList();
                    }
                }
            }
            else
            {
                yield break;
            }

            yield return new WaitForSeconds(m_gameSettings.autoDelayTime);
        }

        m_autoCo = null;
    }

    private void AddCellHasHighestAppearanceOnCurrentAvailableList()
    {
        Dictionary<eCellType, int> typeCountDict = new Dictionary<eCellType, int>();
        foreach (var cell in m_currentAvailableCellList)
        {
            if (typeCountDict.ContainsKey((eCellType)cell.CellTypeInt))
            {
                typeCountDict[(eCellType)cell.CellTypeInt]++;
            }
            else
            {
                typeCountDict[(eCellType)cell.CellTypeInt] = 1;
            }
        }
        eCellType maxType = eCellType.TYPE_ONE;
        int maxCount = 0;
        foreach (var kvp in typeCountDict)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                maxType = kvp.Key;
            }
        }

        foreach (var cell in m_currentAvailableCellList)
        {
            if ((eCellType)cell.CellTypeInt == maxType)
            {
                AddCellToTray(cell);
                break;
            }
        }
    }

    private void AddCellToTray(Cell cell)
    {
        m_TrayController.AddCell(cell);
        UpdateCurrentAvailableCellList(cell);
        OnMoveEvent();
    }

    public void UpdateCurrentAvailableCellList(Cell cell)
    {
        if (cell.CanBeSelected && !m_currentAvailableCellList.Contains(cell))
        {
            m_currentAvailableCellList.Add(cell);
        }
        else if (!cell.CanBeSelected && m_currentAvailableCellList.Contains(cell))
        {
            m_currentAvailableCellList.Remove(cell);
        }
    }

    public bool IsWin { get; set; } = false;

    public bool IsAllBoardCleared()
    {
        foreach (var board in m_boardList)
        {
            foreach (var cell in board.Cells)
            {
                if (cell != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void InitializeAllCellStatuses()
    {
        foreach (var board in m_boardList)
        {
            for (int x = 0; x < board.Size; x++)
            {
                for (int y = 0; y < board.Size; y++)
                {
                    Cell cell = board.GetCellAt(x, y);
                    if (cell != null)
                    {
                        cell.InitializeBlockingStatus();
                        UpdateCurrentAvailableCellList(cell);
                    }
                }
            }
        }
    }

    public int CountBlockingCellsAt(int currentBoardIndex, int row, int col)
    {
        if (currentBoardIndex == m_gameSettings.Height - 1)
        {
            return 0;
        }

        int count = 0;

        // Check only 1 board in front

        Board frontBoard = m_boardList[currentBoardIndex + 1];

        int[] checkRows = { row, row - 1 };
        int[] checkCols = { col, col - 1 };

        foreach (int r in checkRows)
        {
            foreach (int c in checkCols)
            {
                Cell blockingCell = frontBoard.GetCellAt(r, c);
                if (blockingCell != null)
                {
                    count++;
                }
            }
        }

        return count;
    }

    // When a cell is removed, added to tray, notify the 4 cells in front that might be unblocked
    public void NotifyAffectedCellsOnRemove(int boardIndex, int row, int col)
    {
        if (boardIndex == 0) return;

        Board behindBoard = m_boardList[boardIndex - 1];

        int[] affectedRows = { row, row + 1 };
        int[] affectedCols = { col, col + 1 };

        foreach (int r in affectedRows)
        {
            foreach (int c in affectedCols)
            {
                Cell affectedCell = behindBoard.GetCellAt(r, c);
                if (affectedCell != null)
                {
                    affectedCell.OnBlockingCellRemoved();
                    UpdateCurrentAvailableCellList(affectedCell);
                }
            }
        }
    }

    // When a cell is added back, notify the 4 cells in front that are now blocked
    public void NotifyAffectedCellsOnAdd(int boardIndex, int row, int col)
    {
        if (boardIndex == 0) return;

        Board behindBoard = m_boardList[boardIndex - 1];

        int[] affectedRows = { row, row + 1 };
        int[] affectedCols = { col, col + 1 };

        foreach (int r in affectedRows)
        {
            foreach (int c in affectedCols)
            {
                Cell affectedCell = behindBoard.GetCellAt(r, c);
                if (affectedCell != null)
                {
                    affectedCell.OnBlockingCellAdded();
                    UpdateCurrentAvailableCellList(affectedCell);
                }
            }
        }
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.WIN:
                m_gameFinished = true;
                break;
            case GameManager.eStateGame.LOSE:
                m_gameFinished = true;
                break;
        }
    }

    public void Update()
    {
        if (m_gameFinished) return;
        if (IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                Cell c = hit.collider.GetComponent<Cell>();
                if (c != null && c.CanBeSelected && !c.OnTray)
                {
                    m_TrayController.AddCell(c);
                    UpdateCurrentAvailableCellList(c);
                    OnMoveEvent();
                }
                else if (c.OnTray && c.CanAddBackFromTrayToBoard)
                {
                    m_TrayController.ReturnCellToBoard(c);
                    UpdateCurrentAvailableCellList(c);
                    OnMoveEvent();
                }
            }
        }
    }

    public void Clear()
    {
        foreach (var board in m_boardList)
        {
            board.Clear();
        }
        m_boardList.Clear();
        m_TrayController.Clear();
        m_gameManager.StateChangedAction -= OnGameStateChange;
        m_gameManager.OnActivateAutoWinEvent -= GameManager_OnActivateAutoWinEvent;
        m_gameManager.OnActivateAutoLoseEvent -= GameManager_OnActivateAutoLoseEvent;
    }
}