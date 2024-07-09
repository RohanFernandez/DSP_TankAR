using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton instance
    private static GameManager s_Instance = null;
    public static GameManager Instance
    {
        get { return s_Instance; }
    }

    //Manages all tank related information
    [SerializeField]
    private TankManager m_TankManager = null;

    //Manages all UI
    [SerializeField]
    private UIManager m_UIManager = null;

    #region Game State

    public enum GAME_STATE
    { 
        NONE            =   1000,
        ADD_EDIT_TANK   =   0,
        GAMEPLAY        =   1
    }

    [SerializeField]
    private GAME_STATE m_CurrentGameState = GAME_STATE.NONE;
    public GAME_STATE CurrentGameState
    {
        get { return m_CurrentGameState; }
        private set {
            if ((m_CurrentGameState != value) && (value != GAME_STATE.NONE))
            {
                m_CurrentGameState = value;

                m_UIManager.onGameStateChanged(m_CurrentGameState);
            }
        }
    }

    public void setGameState(GAME_STATE a_GameState)
    {
        CurrentGameState = a_GameState;
    }

    #endregion Game State

    public void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize managers on begin
    /// </summary>
    public void Initialize()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
            m_TankManager.initialize();
            m_UIManager.initialize(setGameState);

            CurrentGameState = GAME_STATE.ADD_EDIT_TANK;
        }
    }

    public void OnDestroy()
    {
        s_Instance = null;
    }

    public void resetGame()
    {
        m_TankManager.resetGame();
    }
}
