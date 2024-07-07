using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //Checks so that a single instance would only exist/initialized
    private bool m_bIsInitialized = false;

    [SerializeField]
    private UnityEngine.UI.Toggle m_toggleGameState = null;

    System.Action<GameManager.GAME_STATE> m_actOnGameStateSet = null;

    private const string LABLE_GAMEPLAY = "Add / Reposition Tank\n MODE: OFF";
    private const string LABLE_ADD_REPOSITION_TANK = "Add / Reposition Tank\n MODE: ON";

    // The UI label that displays the number of tanks destroyed
    [SerializeField]
    private TMPro.TMP_Text m_txtTanksDestroyed = null;

    // The UI label that displays the number of tanks currently alive
    [SerializeField]
    private TMPro.TMP_Text m_txtTanksAlive = null;

    [SerializeField]
    private TMPro.TMP_Text m_txtGamestateLable = null;

    public void initialize(System.Action<GameManager.GAME_STATE> a_actOnGameStateSet)
    {
        if (!m_bIsInitialized)
        {
            m_bIsInitialized = true;
            m_actOnGameStateSet = a_actOnGameStateSet;
        }
    }

    public void onToggleClicked_GameState()
    {
        GameManager.GAME_STATE l_GameState = m_toggleGameState.isOn ? GameManager.GAME_STATE.ADD_EDIT_TANK : GameManager.GAME_STATE.GAMEPLAY;

        if (m_actOnGameStateSet != null)
        {
            m_actOnGameStateSet(l_GameState);
        }
    }

    public void onGameStateChanged(GameManager.GAME_STATE a_GameState)
    {
        if (a_GameState == GameManager.GAME_STATE.GAMEPLAY)
        {
            m_toggleGameState.isOn = false;
            m_txtGamestateLable.text = LABLE_GAMEPLAY;
        }
        else {
            m_toggleGameState.isOn = true;
            m_txtGamestateLable.text = LABLE_ADD_REPOSITION_TANK;
        }
    }

    /// <summary>
    /// Updates the UI labels that display the tanks that are alive/destroyed
    /// </summary>
    public void updateTanksAliveDestroyedUILabels(int a_iTanksDestroyed, int a_iTanksActive)
    {
        m_txtTanksDestroyed.text = a_iTanksDestroyed.ToString();
        m_txtTanksAlive.text = a_iTanksActive.ToString();
    }
}
