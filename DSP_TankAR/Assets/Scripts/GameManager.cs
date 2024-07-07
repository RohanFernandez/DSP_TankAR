using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton instance
    private static GameManager s_Instance = null;

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

    // The UI label that displays the number of tanks destroyed
    [SerializeField]
    private TMPro.TMP_Text m_txtTanksDestroyed = null;

    // The UI label that displays the number of tanks currently alive
    [SerializeField]
    private TMPro.TMP_Text m_txtTanksAlive = null;

    // Tanks currently alive
    private int m_iTanksActive = 0;

    // Tanks currently destroyed
    private int m_iTanksDestroyed = 0;

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

            m_TankControllerPool = new MonoObjectPool<TankController>(m_TankControllerPrefab, m_ParentTankHolders, 6);
            m_CanonObjectPool = new MonoObjectPool<RocketProjectile>(m_RocketProjectilePrefab, m_ParentRocketProjectileHolders, 4);
        }
    }

    /// <summary>
    /// Disables all tanks and canon rockets and resets the count to 0
    /// </summary>
    public void resetGame()
    {
        m_TankControllerPool.returnAll();
        m_CanonObjectPool.returnAll();
        m_iTanksActive = 0;
        m_iTanksDestroyed = 0;
        updateTanksAliveDestroyedUILabels();
    }

    /// <summary>
    /// Manage active-destroyed tanks counter. Retrieve a new tank from the pool.
    /// </summary>
    /// <param name="a_v3TankPosition"></param>
    public void addTank(Vector3 a_v3TankPosition)
    {
        TankController l_Tank = m_TankControllerPool.getObject();
        l_Tank.setup(a_v3TankPosition, destroyTank, getRefRocketProjectile);
        ++m_iTanksActive;
        --m_iTanksDestroyed;
        updateTanksAliveDestroyedUILabels();
    }

    public void getRefRocketProjectile(ref RocketProjectile a_CanonRocket)
    {
        a_CanonRocket = m_CanonObjectPool.getObject();
    }

    /// <summary>
    /// Manage active-destroyed tanks counter. Return the tank destroyed back to the pool.
    /// </summary>
    /// <param name="a_Tank"></param>
    public void destroyTank(TankController a_Tank)
    {
        m_TankControllerPool.returnToPool(a_Tank);
        --m_iTanksActive;
        ++m_iTanksDestroyed;
        updateTanksAliveDestroyedUILabels();
    }

    /// <summary>
    /// Updates the UI labels that display the tanks that are alive/destroyed
    /// </summary>
    private void updateTanksAliveDestroyedUILabels()
    {
        m_txtTanksDestroyed.text = m_iTanksDestroyed.ToString();
        m_txtTanksAlive.text = m_iTanksActive.ToString();
    }
}
