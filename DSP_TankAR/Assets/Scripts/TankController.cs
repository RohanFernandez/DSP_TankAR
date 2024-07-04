using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour, IReusable
{
    [SerializeField]
    private float m_fCanonRotationPerSec = 1.0f;

    [SerializeField]
    private Transform m_transCanonRotationAxisTransform = null;

    [SerializeField]
    private Transform m_transCanon = null;

    public void Reset()
    {
        m_transCanon.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Rotates the cannon.
    /// Clockwise/Anti-clockwise with 
    /// </summary>
    /// <param name="a_bIsClockwise"></param>
    public void rotateCannon(bool a_bIsClockwise = true)
    {
        m_transCanon.Rotate(m_transCanonRotationAxisTransform.up, m_fCanonRotationPerSec * Time.deltaTime * (a_bIsClockwise ? -1.0f : 1.0f));
    }

    public void onRetrievedFromPool()
    {
        Reset();
        toggleVisibility(true);
    }

    public void onReturnedToPool()
    {
        toggleVisibility(false);
    }

    /// <summary>
    /// Show/ Hide the tank object
    /// </summary>
    /// <param name="a_bIsVisible"></param>
    private void toggleVisibility(bool a_bIsVisible = true)
    {
        gameObject.SetActive(a_bIsVisible);
    }
}
