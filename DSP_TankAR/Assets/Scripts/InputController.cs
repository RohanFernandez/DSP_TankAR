using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

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
    private float m_ViewportPeriphery = 0.15f;

    private ETouch.Finger m_FingerLeft = null;
    private ETouch.Finger m_FingerRight = null;

    [SerializeField]
    RectTransform m_RightRectJoystickArea = null;

    [SerializeField]
    RectTransform m_RightRectJoystickKnob = null;

    [SerializeField]
    private Vector2 m_RightDirectionMovement = Vector2.zero;
    public Vector2 RightJoystickMovement
    {
        get { return m_RightDirectionMovement; }
    }

    [SerializeField]
    RectTransform m_LeftRectJoystickArea = null;

    [SerializeField]
    RectTransform m_LeftRectJoystickKnob = null;

    [SerializeField]
    private Vector2 m_LeftDirectionMovement = Vector2.zero;
    public Vector2 LeftJoystickMovement
    {
        get { return m_LeftDirectionMovement; }
    }

    [SerializeField]
    private UIManager m_UiManager = null;

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

    private void OnEnable()
    {
        ETouch.EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += callbackOnTouchOnFingerDown;
        ETouch.Touch.onFingerUp += callbackOnTouchOnFingerUp;
        ETouch.Touch.onFingerMove += callbackOnTouchOnFingerMove;
    }

    private void OnDisable()
    {
        ETouch.Touch.onFingerDown -= callbackOnTouchOnFingerDown;
        ETouch.Touch.onFingerUp -= callbackOnTouchOnFingerUp;
        ETouch.Touch.onFingerMove -= callbackOnTouchOnFingerMove;
        ETouch.EnhancedTouchSupport.Disable();
    }

    private void callbackOnTouchOnFingerDown(ETouch.Finger a_Finger)
    {
        ETouch.Finger l_CurrentFinger = null;
        RectTransform l_rectCurrentJoystickArea = null;
        RectTransform l_rectCurrentJoystickKnob = null;
        Vector2 l_v3JoystickAreaScreenPosition = Vector2.zero;
        float l_fDistanceFromKnobCenter = 0.0f;

        bool l_bIsLeftFinger = true;

        if (a_Finger.screenPosition.x > (Screen.width * 0.5f))
        {
            l_v3JoystickAreaScreenPosition = m_RightRectJoystickArea.TransformPoint(m_RightRectJoystickArea.rect.center);
            float l_RectHalf = m_RightRectJoystickArea.rect.width * 0.5f;
            l_fDistanceFromKnobCenter = Vector2.Distance(a_Finger.screenPosition, l_v3JoystickAreaScreenPosition);

            //Right finger
            if ((m_FingerRight == null) && (l_fDistanceFromKnobCenter < l_RectHalf))
            {
                m_FingerRight = a_Finger;
                l_CurrentFinger = a_Finger;
                l_rectCurrentJoystickArea = m_RightRectJoystickArea;
                l_rectCurrentJoystickKnob = m_RightRectJoystickKnob;
                l_bIsLeftFinger = false;
            }
        }
        else
        {
            l_v3JoystickAreaScreenPosition = m_LeftRectJoystickArea.TransformPoint(m_LeftRectJoystickArea.rect.center);
            float l_RectHalf = m_LeftRectJoystickArea.rect.width * 0.5f;
            l_fDistanceFromKnobCenter = Vector2.Distance(a_Finger.screenPosition, l_v3JoystickAreaScreenPosition);

            //Left finger
            if (m_FingerLeft == null && (l_fDistanceFromKnobCenter < l_RectHalf))
            {
                m_FingerLeft = a_Finger;
                l_CurrentFinger = a_Finger;
                l_rectCurrentJoystickArea = m_LeftRectJoystickArea;
                l_rectCurrentJoystickKnob = m_LeftRectJoystickKnob;
            }
        }

        if (l_CurrentFinger != null)
        {
            float l_RectHalf = l_rectCurrentJoystickArea.rect.width * 0.5f;
            Vector2 l_v2MoveDirection = (l_CurrentFinger.screenPosition - l_v3JoystickAreaScreenPosition) / l_RectHalf;
            if (l_bIsLeftFinger)
            {
                m_LeftDirectionMovement = l_v2MoveDirection;
            }
            else
            {
                m_RightDirectionMovement = l_v2MoveDirection;
            }
            l_rectCurrentJoystickKnob.anchoredPosition = l_v2MoveDirection * l_RectHalf;
        }
        else
        {
            if (m_FingerRight != null || m_FingerLeft != null)
            {
                RectTransform l_rectBtn = m_UiManager.BtnFire.GetComponent<RectTransform>();
                l_rectBtn.rect.Contains(a_Finger.screenPosition);
                m_UiManager.BtnFire.onClick.Invoke();
            }
        }
    }

    private void callbackOnTouchOnFingerUp(ETouch.Finger a_Finger)
    {
        ETouch.Finger l_CurrentFinger = null;
        RectTransform l_rectCurrentJoystickKnob = null;
        RectTransform l_rectCurrentJoystickArea = null;
        bool l_bIsLeftFinger = true;

        if (a_Finger.screenPosition.x > (Screen.width * 0.5f))
        {
            //Right finger
            if (m_FingerRight != null)
            {
                m_FingerRight = null;
                l_CurrentFinger = a_Finger;
                l_rectCurrentJoystickKnob = m_RightRectJoystickKnob;
                l_rectCurrentJoystickArea = m_RightRectJoystickArea;
                l_bIsLeftFinger = false;
            }
        }
        else
        {
            //Left finger
            if (m_FingerLeft != null)
            {
                m_FingerLeft = null;
                l_CurrentFinger = a_Finger;
                l_rectCurrentJoystickKnob = m_LeftRectJoystickKnob;
                l_rectCurrentJoystickArea = m_LeftRectJoystickArea;
            }
        }

        if (l_CurrentFinger != null)
        {
            if (l_bIsLeftFinger)
            {
                m_LeftDirectionMovement = Vector2.zero;
            }
            else
            {
                m_RightDirectionMovement = Vector2.zero;
            }
            l_rectCurrentJoystickKnob.anchoredPosition = Vector2.zero;
        }
    }

    private void callbackOnTouchOnFingerMove(ETouch.Finger a_Finger)
    {
        ETouch.Finger l_CurrentFinger = null;
        RectTransform l_rectCurrentJoystickArea = null;
        RectTransform l_rectCurrentJoystickKnob = null;
        bool l_bIsLeftFinger = true;

        if (a_Finger.screenPosition.x > (Screen.width * 0.5f))
        {
            //Right finger
            if (m_FingerRight != null)
            {
                m_FingerRight = a_Finger;
                l_CurrentFinger = a_Finger;
                l_rectCurrentJoystickArea = m_RightRectJoystickArea;
                l_rectCurrentJoystickKnob = m_RightRectJoystickKnob;
                l_bIsLeftFinger = false;
            }
        }
        else
        {
            //Left finger
            if (m_FingerLeft != null)
            {
                m_FingerLeft = a_Finger;
                l_CurrentFinger = a_Finger;
                l_rectCurrentJoystickArea = m_LeftRectJoystickArea;
                l_rectCurrentJoystickKnob = m_LeftRectJoystickKnob;
            }
        }

        if (l_CurrentFinger != null)
        {
            float l_fJoystickHalfAreaDistance = (l_rectCurrentJoystickArea.rect.width * 0.5f);
            Vector2 l_v3JoystickAreaScreenPosition = l_rectCurrentJoystickArea.TransformPoint(l_rectCurrentJoystickArea.rect.center);
            Vector2 l_v2Move = (l_CurrentFinger.screenPosition - l_v3JoystickAreaScreenPosition);

            Vector2 l_v2MoveDirection = l_v2Move.normalized;
            float l_fMoveDistance = Mathf.Min(l_v2Move.magnitude, l_fJoystickHalfAreaDistance);

            if (l_bIsLeftFinger)
            {
                m_LeftDirectionMovement = (l_v2MoveDirection * l_fMoveDistance) / l_fJoystickHalfAreaDistance;
            }
            else
            {
                m_RightDirectionMovement = (l_v2MoveDirection * l_fMoveDistance) / l_fJoystickHalfAreaDistance;
            }
            l_rectCurrentJoystickKnob.anchoredPosition = (l_v2MoveDirection * l_fMoveDistance);
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
