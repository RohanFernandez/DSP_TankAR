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
    Camera m_Camera;

    public void initialize()
    {
        if (!m_bIsInitialized)
        {
            m_bIsInitialized = true;
            m_TankControllerPool = new MonoObjectPool<TankController>(m_TankControllerPrefab, m_ParentTankHolders, 6);
            m_CanonObjectPool = new MonoObjectPool<RocketProjectile>(m_RocketProjectilePrefab, m_ParentRocketProjectileHolders, 4);

            m_SpawnAction.EnableDirectAction();
            m_ARInteractor = m_ARInteractorObject as IARInteractor;
        }
    }

    public void OnDestroy()
    {
        m_SpawnAction.DisableDirectAction();
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
    /// Updates the tank alive/destroyed values in the UI manager
    /// </summary>
    private void updateTanksAliveDestroyedUILabels()
    {
       m_UIManager.updateTanksAliveDestroyedUILabels(m_iTanksActive, m_iTanksDestroyed);
    }

    #region TANK SPAWNER
    [SerializeField]
    InputActionProperty m_SpawnAction = new(new InputAction(type: InputActionType.Button));

    IARInteractor m_ARInteractor;

    [SerializeField]
    private Object m_ARInteractorObject;

    [SerializeField]
    [Tooltip("The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
    float m_ViewportPeriphery = 0.15f;

    void Update()
    {
        if (m_SpawnAction.action.WasPerformedThisFrame() && (GameManager.Instance.CurrentGameState == GameManager.GAME_STATE.ADD_EDIT_TANK))
        {
            if (m_ARInteractor.TryGetCurrentARRaycastHit(out var arRaycastHit))
            {
                var arPlane = arRaycastHit.trackable as ARPlane;
                if (arPlane == null)
                    return;

                if (arPlane.alignment != UnityEngine.XR.ARSubsystems.PlaneAlignment.HorizontalUp)
                    return;

                //m_ObjectSpawner.TrySpawnObject(arRaycastHit.pose.position, arPlane.normal);

                
                var inViewMin = m_ViewportPeriphery;
                var inViewMax = 1.0f - m_ViewportPeriphery;
                var pointInViewportSpace = m_Camera.WorldToViewportPoint(arRaycastHit.pose.position);
                if (pointInViewportSpace.z < 0.0f || pointInViewportSpace.x > inViewMax || pointInViewportSpace.x < inViewMin ||
                    pointInViewportSpace.y > inViewMax || pointInViewportSpace.y < inViewMin)
                {
                    return;
                }

                addTank(arRaycastHit.pose.position);

                //var facePosition = m_CameraToFace.transform.position;
                //var forward = facePosition - spawnPoint;
                //BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
                //newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);

                //if (m_ApplyRandomAngleAtSpawn)
                //{
                //    var randomRotation = Random.Range(-m_SpawnAngleRange, m_SpawnAngleRange);
                //    newObject.transform.Rotate(Vector3.up, randomRotation);
                //}

                //if (m_SpawnVisualizationPrefab != null)
                //{
                //    var visualizationTrans = Instantiate(m_SpawnVisualizationPrefab).transform;
                //    visualizationTrans.position = spawnPoint;
                //    visualizationTrans.rotation = newObject.transform.rotation;
                //}
            }
        }
    }

    #endregion TANK SPAWNER
}
