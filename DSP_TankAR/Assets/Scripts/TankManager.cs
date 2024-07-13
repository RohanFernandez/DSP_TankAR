using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

public class TankManager : MonoBehaviour
{
    //Checks so that a single instance would only exist/initialized
    private bool m_bIsInitialized = false;

    // Prefab object of the tank controller that needs to be instantiated
    [SerializeField]
    private TankController m_TankControllerPrefab = null;

    // Prefab object of the canon rocket that needs to be instantiated
    [SerializeField]
    private RocketProjectile m_RocketProjectilePrefab = null;

    // Parent gameobject of the tanks that would be instantiated as children
    [SerializeField]
    private GameObject m_ParentTankHolders = null;

    // Parent gameobject of the rocket projectile that would be instantiated as children
    [SerializeField]
    private GameObject m_ParentRocketProjectileHolders = null;

    // The mono object pool that manages the pool of tank objects
    private MonoObjectPool<TankController> m_TankControllerPool = null;

    // The mono object pool that manages the pool of rocket projectile objects
    private MonoObjectPool<RocketProjectile> m_CanonObjectPool = null;

    // Tanks currently alive
    private int m_iTanksActive = 0;

    // Tanks currently destroyed
    private int m_iTanksDestroyed = 0;

    [SerializeField]
    private UIManager m_UIManager = null;

    // The current tank currently under the player's control
    private TankController m_CurrentControlledTank = null;

    [SerializeField]
    private InputController m_InputController = null;

    [SerializeField]
    private Camera m_Camera = null;

    public void initialize()
    {
        if (!m_bIsInitialized)
        {
            m_bIsInitialized = true;
            m_TankControllerPool = new MonoObjectPool<TankController>(m_TankControllerPrefab, m_ParentTankHolders, 6);
            m_CanonObjectPool = new MonoObjectPool<RocketProjectile>(m_RocketProjectilePrefab, m_ParentRocketProjectileHolders, 4);
        }
    }

    /// <summary>
    /// Manage active-destroyed tanks counter. Retrieve a new tank from the pool.
    /// </summary>
    /// <param name="a_v3TankPosition"></param>
    public void addTank(Vector3 a_v3TankPosition, Camera a_Camera)
    {
        TankController l_Tank = m_TankControllerPool.getObject();
        l_Tank.setup(a_v3TankPosition, a_Camera, destroyTank, getRefRocketProjectile, returnRefRocketProjectile, onTankControllerSelected);
        ++m_iTanksActive;
        updateTanksAliveDestroyedUILabels();

        if (m_iTanksActive == 1)
        {
            onTankControllerSelected(l_Tank);
        }
    }

    public void getRefRocketProjectile(ref RocketProjectile a_CanonRocket)
    {
        a_CanonRocket = m_CanonObjectPool.getObject();
    }

    public void returnRefRocketProjectile(RocketProjectile a_CanonRocket)
    {
        m_CanonObjectPool.returnToPool(a_CanonRocket);
    }


    public void onTankControllerSelected(TankController a_TankController)
    {
        if (a_TankController == m_CurrentControlledTank && m_CurrentControlledTank != null)
        {
            return;
        }

        if (m_CurrentControlledTank != null)
        {
            m_CurrentControlledTank.toggleSelector(false);
        }
        m_CurrentControlledTank = a_TankController;
        if (m_CurrentControlledTank != null)
        {
            m_CurrentControlledTank.toggleSelector(true);
        }

        if ((m_CurrentControlledTank != null) && (GameManager.Instance.CurrentGameState == GameManager.GAME_STATE.GAMEPLAY))
        {
            //show UI controller panel
            m_UIManager.toggleUIBottomPanel(true);
        }
        else
        {
            //Hide UI controller panel
            m_UIManager.toggleUIBottomPanel(false);
        }
    }

    /// <summary>
    /// Manage active-destroyed tanks counter. Return the tank destroyed back to the pool.
    /// </summary>
    /// <param name="a_Tank"></param>
    public void destroyTank(TankController a_Tank)
    {
        --m_iTanksActive;
        ++m_iTanksDestroyed;
        updateTanksAliveDestroyedUILabels();

        if (m_CurrentControlledTank == a_Tank)
        {
            onTankControllerSelected(null);
        }
    }

    /// <summary>
    /// Disables all tanks and canon rockets and resets the count to 0
    /// </summary>
    public void resetGame()
    {
        m_TankControllerPool.returnAll();
        onTankControllerSelected(null);
        m_CanonObjectPool.returnAll();
        m_iTanksActive = 0;
        m_iTanksDestroyed = 0;
        updateTanksAliveDestroyedUILabels();
    }

    /// <summary>
    /// Updates the tank alive/destroyed values in the UI manager
    /// </summary>
    private void updateTanksAliveDestroyedUILabels()
    {
       m_UIManager.updateTanksAliveDestroyedUILabels(m_iTanksActive, m_iTanksDestroyed);
    }

    /// <summary>
    /// Callback on canon fire button clicked
    /// </summary>
    public void shootRocketFromSelectedTank()
    {
        if (m_CurrentControlledTank != null)
        {
            m_CurrentControlledTank.shootCanon();
        }
    }

    public void onGameStateChanged(GameManager.GAME_STATE a_GameState)
    {
        if (a_GameState == GameManager.GAME_STATE.GAMEPLAY)
        {
            if (m_CurrentControlledTank != null)
            {
                m_UIManager.toggleUIBottomPanel(true);
            }
        }
    }

    /// <summary>
    /// Handles the joystick to canon rotation/ tank movement
    /// </summary>
    private void Update()
    {
        if ((GameManager.Instance.CurrentGameState == GameManager.GAME_STATE.GAMEPLAY) && (m_CurrentControlledTank != null))
        {
            if (m_InputController.RightJoystickMovement != Vector2.zero)
            {
                m_CurrentControlledTank.rotateCannon(m_Camera.transform.TransformDirection(m_InputController.RightJoystickMovement.normalized));
            }

            if (m_InputController.LeftJoystickMovement != Vector2.zero)
            {
                m_CurrentControlledTank.moveInDirection(m_Camera.transform.TransformDirection(m_InputController.LeftJoystickMovement.normalized));
            }
        }
    }
}
