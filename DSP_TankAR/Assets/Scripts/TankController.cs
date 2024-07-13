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

    //Callback on returning rocket back to the pool
    System.Action<RocketProjectile> m_actOnReturnRocket = null;

    // Callback to get a canon rocket
    public delegate void ReferencedAction<T>(ref T referencedItem);
    private ReferencedAction<RocketProjectile> m_actCallbackGetCanonRocket = null;

    //XR grabable component on the tank
    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable m_Interactable = null;

    //The Sprite UI that indicates that this tank is the selected tank
    [SerializeField]
    private GameObject m_Selector = null;

    //The positition in the forward of the canon where the rocket will be spawned to then be projected forward
    [SerializeField]
    private GameObject m_RocketSpawnPosition = null;

    
    // Max rotation of the tank
    [SerializeField]
    private float m_fMaxMoveRotaion = 20.0f;

    //Rigidbody attached to the tank
    [SerializeField]
    private Rigidbody m_RigidBody = null;

    //Tank movement speed
    [SerializeField]
    private float m_fTankSpeed = 20.0f;

    // Is the tank destroyed
    private bool m_bIsDestroyed = false;
    public bool IsDestroyed { get { return m_bIsDestroyed; } }

    //The Sprite UI that indicates that this tank is destroyed
    [SerializeField]
    private GameObject m_Destroyed = null;

    /// <summary>
    /// Rotates the cannon.
    /// Clockwise/Anti-clockwise with 
    /// </summary>
    /// <param name="a_bIsClockwise"></param>
    public void rotateCannon(Vector3 a_JoystickDirection)
    {
        UnityEngine.XR.Interaction.Toolkit.Utilities.BurstMathUtility.ProjectOnPlane(m_transCanon.forward, Vector3.up, out var l_outCanonProjectedForward);
        UnityEngine.XR.Interaction.Toolkit.Utilities.BurstMathUtility.ProjectOnPlane(a_JoystickDirection.normalized, Vector3.up, out var l_outJoystickProjectedForward);

        Quaternion l_quatFrom = Quaternion.LookRotation(l_outCanonProjectedForward, m_transCanonRotationAxisTransform.up);
        Quaternion l_quatTo = Quaternion.LookRotation(l_outJoystickProjectedForward.normalized, m_transCanonRotationAxisTransform.up);

        m_transCanon.transform.rotation = Quaternion.RotateTowards(l_quatFrom, l_quatTo, Time.deltaTime * m_fCanonRotationPerSec);
    }

    void IReusable.onRetrievedFromPool()
    {
        toggleVisibility(true);
        toggleSelector(false);
        m_Interactable.firstSelectEntered.AddListener(OnFirstSelectEntered);
        m_bIsDestroyed = false;
        m_Destroyed.SetActive(false);
        m_Interactable.enabled = true;
    }

    void IReusable.onReturnedToPool()
    {
        toggleVisibility(false);
        toggleSelector(false);
        m_Interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
    }

    private void OnFirstSelectEntered(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs a_Args)
    {
        onSelected();
    }

    /// <summary>
    /// Callback on selecting the tank
    /// </summary>
    public void onSelected()
    {
        if (m_bIsDestroyed) { return; }
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
        m_transCanon.localRotation = Quaternion.identity;

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
        if (m_bIsDestroyed) { return; }

        m_Interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
        m_Destroyed.SetActive(true);
        m_Interactable.enabled = false;
        m_bIsDestroyed = true;

        if (m_actCallbackOnHit != null)
        {
            m_actCallbackOnHit(this);
        }
    }

    /// <summary>
    /// Spawns a rocket and shoots it with the current forward direction of the canon as the direction of the rocket
    /// </summary>
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

    /// <summary>
    /// Moves the tank with the velocity
    /// </summary>
    /// <param name="a_v3Velocity"></param>
    public void moveInDirection(Vector3 a_v3Velocity)
    {
        UnityEngine.XR.Interaction.Toolkit.Utilities.BurstMathUtility.ProjectOnPlane(a_v3Velocity.normalized, Vector3.up, out var l_OutMovementDirection);
        UnityEngine.XR.Interaction.Toolkit.Utilities.BurstMathUtility.ProjectOnPlane(transform.forward, Vector3.up, out var l_OutTankProjectedForward);

        float l_fDirectionMultiplier = Vector3.Dot(l_OutMovementDirection.normalized, l_OutTankProjectedForward.normalized) > -0.75f ? 1.0f : -1.0f;

        Quaternion l_quatFrom = Quaternion.LookRotation(l_OutTankProjectedForward, m_transCanonRotationAxisTransform.up);
        Quaternion l_quatTo = Quaternion.LookRotation(l_OutMovementDirection.normalized * l_fDirectionMultiplier, m_transCanonRotationAxisTransform.up);

        transform.rotation = Quaternion.RotateTowards(l_quatFrom, l_quatTo, Time.deltaTime * m_fMaxMoveRotaion);

        m_RigidBody.AddForce(transform.forward * l_fDirectionMultiplier * m_fTankSpeed * Time.deltaTime, ForceMode.VelocityChange);
    }
}
