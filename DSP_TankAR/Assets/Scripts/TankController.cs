using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour, IReusable
{
    // Rotation of canon turret
    [SerializeField]
    private float m_fCanonRotationPerSec = 1.0f;

    // The axis of the canon at which it rotates around
    [SerializeField]
    private Transform m_transCanonRotationAxisTransform = null;

    // The canon gameobject
    [SerializeField]
    private Transform m_transCanon = null;

    // Callback on when this tank is hit by a rocket
    System.Action<TankController> m_actCallbackOnHit = null;

    // Callback on when this tank is selected
    System.Action<TankController> m_actOnTankSelected = null;

    System.Action<RocketProjectile> m_actOnReturnRocket = null;

    // Callback to get a canon rocket
    public delegate void ReferencedAction<T>(ref T referencedItem);
    private ReferencedAction<RocketProjectile> m_actCallbackGetCanonRocket = null;

    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable m_Interactable = null;

    [SerializeField]
    private GameObject m_Selector = null;

    [SerializeField]
    private GameObject m_RocketSpawnPosition = null;

    // Reset the canon on spawn. Resets the Canon rotation
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

    void IReusable.onRetrievedFromPool()
    {
        Reset();
        toggleVisibility(true);
        toggleSelector(false);
        m_Interactable.firstSelectEntered.AddListener(OnFirstSelectEntered);
    }

    void IReusable.onReturnedToPool()
    {
        toggleVisibility(false);
        toggleSelector(false);
        m_Interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
    }

    protected virtual void OnFirstSelectEntered(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (m_actOnTankSelected != null)
        {
            m_actOnTankSelected(this);
        }
    }

    /// <summary>
    /// Show/ Hide the tank object
    /// </summary>
    /// <param name="a_bIsVisible"></param>
    private void toggleVisibility(bool a_bIsVisible = true)
    {
        gameObject.SetActive(a_bIsVisible);
    }

    /// <summary>
    /// Sets up the tank on spawn with a position and a callback on hit
    /// </summary>
    /// <param name="a_v3Position"></param>
    public void setup(Vector3 a_v3Position, Camera a_Camera, System.Action<TankController> callbackOnHit, ReferencedAction<RocketProjectile> callbackGetCanonRocket, System.Action<RocketProjectile> callbackOnReturnRocket, System.Action<TankController> callbackOnSelected)
    {
        transform.position = a_v3Position;
        UnityEngine.XR.Interaction.Toolkit.Utilities.BurstMathUtility.ProjectOnPlane(a_Camera.transform.position - a_v3Position, Vector3.up, out var l_OutProjectedForward);
        transform.localRotation = Quaternion.LookRotation(l_OutProjectedForward, Vector3.up);

        m_actCallbackOnHit = callbackOnHit;
        m_actCallbackGetCanonRocket = callbackGetCanonRocket;
        m_actOnTankSelected = callbackOnSelected;
        m_actOnReturnRocket = callbackOnReturnRocket;
    }

    /// <summary>
    /// Callback called on tank being hit by a canon rocket
    /// </summary>
    public void onTankHit()
    {
        if (m_actCallbackOnHit != null)
        {
            m_actCallbackOnHit(this);
        }
    }

    public void shootCanon()
    {
        RocketProjectile l_RocketProjectile = null;
        m_actCallbackGetCanonRocket(ref l_RocketProjectile);

        if (l_RocketProjectile != null)
        {
            l_RocketProjectile.transform.SetParent(m_RocketSpawnPosition.transform);
            l_RocketProjectile.transform.localPosition = Vector3.zero;
            l_RocketProjectile.transform.localRotation = Quaternion.identity;
            l_RocketProjectile.setup(this, m_actOnReturnRocket);
        }
    }

    /// <summary>
    /// Show/hide the selector
    /// </summary>
    /// <param name="a_bIsSelected"></param>
    public void toggleSelector(bool a_bIsSelected)
    {
        m_Selector.SetActive(a_bIsSelected);
    }

    public bool mbShoot = false;
    void Update()
    {
        if (mbShoot)
        {
            shootCanon();
            mbShoot = false;
        }
    }
}
