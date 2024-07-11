using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectile : MonoBehaviour, IReusable
{
    // The tank ref that shot the canon
    private TankController m_TankShooter = null;

    // The time the canon rocket will be active for
    private float m_fCanonActivationTime = 4.0f;

    // The velocity the canon is shot with
    [SerializeField]
    private float m_fCanonShootSpeed = 2.0f;

    // Action on completion of the canon rocket movement
    private System.Action<RocketProjectile> m_actOnCanonExistanceEnd = null;

    /// <summary>
    /// Sets up the canon rocket when it is spawned and then fired
    /// </summary>
    /// <param name="a_TankShooter"></param>
    /// <param name="a_actOnCanonExistanceEnd"></param>
    public void setup(TankController a_TankShooter, System.Action<RocketProjectile> a_actOnCanonExistanceEnd)
    {
        m_TankShooter = a_TankShooter;
        m_actOnCanonExistanceEnd = a_actOnCanonExistanceEnd;
        m_fCurrentActiveTime = 0.0f;
    }

    void IReusable.onRetrievedFromPool()
    {
        gameObject.SetActive(true);
    }

    void IReusable.onReturnedToPool()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Callback on the bullet intersecting with a tank collider
    /// </summary>
    /// <param name="other"></param>
    public void OnTriggerEnter(Collider other)
    {
        TankController l_TankHit = other.GetComponent<TankController>();
        if((l_TankHit != null) && (l_TankHit != m_TankShooter))
        {
            l_TankHit.onTankHit();
            if (m_actOnCanonExistanceEnd != null)
            {
                m_actOnCanonExistanceEnd(this);
            }
        }
    }

    float m_fCurrentActiveTime = 0.0f;
    private void Update()
    {
        m_fCurrentActiveTime += Time.deltaTime;
        transform.position += transform.forward * m_fCanonShootSpeed * Time.deltaTime;

        if (m_fCurrentActiveTime > m_fCanonActivationTime)
        {
            if (m_actOnCanonExistanceEnd != null)
            {
                m_actOnCanonExistanceEnd(this);
            }
        }
    }
}
