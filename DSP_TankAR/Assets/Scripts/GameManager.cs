using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton instance
    private static GameManager s_Instance = null;

    [SerializeField]
    private TankController m_TankControllerPrefab = null;

    [SerializeField]
    private GameObject m_ParentTankHolders = null;

    private MonoObjectPool<TankController> m_TankControllerPool = null;

    public void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (s_Instance == null)
        {
            s_Instance = this;

            m_TankControllerPool = new MonoObjectPool<TankController>(m_TankControllerPrefab, m_ParentTankHolders, 6);
        }
    }






}
