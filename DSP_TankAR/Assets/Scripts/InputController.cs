using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class InputController : MonoBehaviour
{

    //Checks so that a single instance would only exist/initialized
    private bool m_bIsInitialized = false;

    private TankManager m_TankManager = null;

    [SerializeField]
    InputActionProperty m_SpawnAction = new(new InputAction(type: InputActionType.Button));

    IARInteractor m_ARInteractor;
    XRBaseControllerInteractor m_ARInteractorAsControllerInteractor;
    bool m_EverHadSelection;

    [SerializeField]
    Camera m_Camera;

    [SerializeField]
    private Object m_ARInteractorObject;

    [SerializeField]
    [Tooltip("The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
    float m_ViewportPeriphery = 0.15f;

    public void initialize(TankManager a_TankManager)
    {
        if (!m_bIsInitialized)
        {
            m_TankManager = a_TankManager;
            m_ARInteractor = m_ARInteractorObject as IARInteractor;
            m_ARInteractorAsControllerInteractor = m_ARInteractorObject as XRBaseControllerInteractor;
            m_SpawnAction.EnableDirectAction();
        }
    }

    private void OnDestroy()
    {
        m_SpawnAction.DisableDirectAction();
    }

    void Update()
    {
        var attemptSpawn = false;
        var currentControllerState = m_ARInteractorAsControllerInteractor.xrController.currentControllerState;
        if (currentControllerState.selectInteractionState.activatedThisFrame)
        {
            m_EverHadSelection = m_ARInteractorAsControllerInteractor.hasSelection;
        }
        else if (currentControllerState.selectInteractionState.active)
        {
            m_EverHadSelection |= m_ARInteractorAsControllerInteractor.hasSelection;
        }
        else if (currentControllerState.selectInteractionState.deactivatedThisFrame)
        {
            attemptSpawn = !m_ARInteractorAsControllerInteractor.hasSelection && !m_EverHadSelection;
        }

        if (attemptSpawn && (GameManager.Instance.CurrentGameState == GameManager.GAME_STATE.ADD_EDIT_TANK))
        {
            ARRaycastHit l_outARRaycastHit;
            if (m_ARInteractor.TryGetCurrentARRaycastHit(out l_outARRaycastHit))
            {
                ARPlane l_ARPlane = (ARPlane)l_outARRaycastHit.trackable;

                if (l_ARPlane == null)
                    return;

                float inViewMin = m_ViewportPeriphery;
                float inViewMax = 1.0f - m_ViewportPeriphery;
                Vector3 l_v3PointInViewportSpace = m_Camera.WorldToViewportPoint(l_outARRaycastHit.pose.position);
                if (l_v3PointInViewportSpace.z < 0.0f || l_v3PointInViewportSpace.x > inViewMax || l_v3PointInViewportSpace.x < inViewMin ||
                    l_v3PointInViewportSpace.y > inViewMax || l_v3PointInViewportSpace.y < inViewMin)
                {
                    return;
                }

                m_TankManager.addTank(l_outARRaycastHit.pose.position, m_Camera);
            }
        }
    }
}
