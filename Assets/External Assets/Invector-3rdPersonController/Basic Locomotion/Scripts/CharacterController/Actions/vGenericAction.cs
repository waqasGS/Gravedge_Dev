using System;
using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController.vActions
{
    using System.Collections.Generic;
    using vCharacterController;

    [vClassHeader("GENERIC ACTION", "Use the vTriggerGenericAction to trigger a simple animation.\n<b><size=12>You can use <color=red>vGenericActionReceiver</color> component to filter events by action name</size></b>", iconName = "triggerIcon")]
    public class vGenericAction : vActionListener
    {
        #region Variables 

        [vEditorToolbar("Settings")]
        [Tooltip("Tag of the object you want to access")]
        public string actionTag = "Action";
        [Tooltip("Use root motion of the animation")]
        public bool useRootMotion = true;

        [vEditorToolbar("Debug")]
        [Header("--- Debug Only ---")]
        [Tooltip("Check this to enter the debug mode")]
        public bool debugMode;
        [vReadOnly] protected vTriggerGenericAction _triggerAction;
        public virtual vTriggerGenericAction triggerAction { get => _triggerAction; set => _triggerAction = value; }
        
        [vReadOnly, SerializeField]
        protected bool _playingAnimation;
        [vReadOnly, SerializeField]
        protected bool actionStarted;
        [vReadOnly]
        public bool isLockTriggerEvents;
        [vReadOnly, SerializeField]
        protected List<Collider> colliders = new List<Collider>();

        [vEditorToolbar("Events")]      
        public vOnActionHandle OnDoAction = new vOnActionHandle();
        public vOnActionHandle OnEnterTriggerAction;
        public vOnActionHandle OnExitTriggerAction;
        public vOnActionHandle OnStartAction;
        public vOnActionHandle OnCancelAction;
        public vOnActionHandle OnEndAction;

        public bool doingAction { get; set; }
        public virtual Camera mainCamera { get; set; }
        public virtual vThirdPersonInput tpInput { get; set; }
        protected virtual float _currentInputDelay { get; set; }
        protected virtual Vector3 _screenCenter { get; set; }
        protected virtual float timeInTrigger { get; set; }
        protected virtual float animationBehaviourDelay { get; set; }

        protected bool finishRotationMatch;
        protected bool finishPositionXZMatch;
        protected bool finishPositionYMatch;
        protected Vector3 animationStartPosition;
        protected Vector3 animationRootMotionDelta;
        protected Vector3 playerModelStartPosition;
        protected bool hasSnappedToRoot;
        protected float snapToRootDelayTimer;
        protected bool isWaitingForSnapDelay;
        protected virtual Vector3 screenCenter
        {
            get
            {
                var center = _screenCenter;
                center.x = Screen.width * 0.5f;
                center.y = Screen.height * 0.5f;
                center.z = 0;
                return _screenCenter = center;
            }
        }

        internal Dictionary<Collider, ActionStorage> actions;

        #endregion

        internal class ActionStorage
        {
            internal vTriggerGenericAction action;
            internal bool isValid;
            internal ActionStorage()
            {

            }
            internal ActionStorage(vTriggerGenericAction action)
            {
                this.action = action;
                action.OnValidate.AddListener((GameObject o) => { isValid = true; });
                action.OnInvalidate.AddListener((GameObject o) => { isValid = false; });
            }
            public static implicit operator vTriggerGenericAction(ActionStorage storage)
            {
                return storage.action;
            }
            public static implicit operator ActionStorage(vTriggerGenericAction action)
            {
                return new ActionStorage(action);
            }
        }

        protected override void SetUpListener()
        {
            actionEnter = true;
            actionStay = true;
            actionExit = true;
            actions = new Dictionary<Collider, ActionStorage>();
        }

        protected override void Start()
        {
            base.Start();
            tpInput = GetComponent<vThirdPersonInput>();

            var actionsReceivers = GetComponentsInChildren<IActionReceiver>();

            for (int i = 0; i < actionsReceivers.Length; i++)
            {
                OnDoAction.AddListener(actionsReceivers[i].OnReceiveAction);
            }


            if (tpInput != null)
            {
                tpInput.onUpdate -= CheckForTriggerAction;
                tpInput.onUpdate += CheckForTriggerAction;

                tpInput.onLateUpdate -= UpdateGenericAction;
                tpInput.onLateUpdate += UpdateGenericAction;
            }
            if (!mainCamera)
            {
                mainCamera = Camera.main;
            }
        }

        protected virtual void UpdateGenericAction()
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
            }

            if (!mainCamera)
            {
                return;
            }

            AnimationBehaviour();
            HandleColliders();
        }

        protected virtual void HandleColliders()
        {
            colliders.Clear();
            foreach (var key in actions.Keys)
            {
                colliders.Add(key);
            }
            if (!doingAction && triggerAction && !isLockTriggerEvents)
            {
                if (timeInTrigger <= 0)
                {
                    actions.Clear();
                    triggerAction = null;
                }
                else
                {
                    timeInTrigger -= Time.deltaTime;
                }
            }
        }

        protected virtual bool inActionAnimation
        {
            get
            {
                return !string.IsNullOrEmpty(triggerAction.playAnimation)
                    && tpInput.cc.animatorStateInfos.stateInfos[triggerAction.animatorLayer].shortPathHash.Equals(Animator.StringToHash(triggerAction.playAnimation));
            }
        }

        protected virtual void CheckForTriggerAction()
        {
            if (actions.Count == 0 && !triggerAction || isLockTriggerEvents)
            {
                return;
            }

            vTriggerGenericAction _triggerAction = GetNearAction();
            if (!doingAction && triggerAction != _triggerAction)
            {
                triggerAction = _triggerAction;
                if (triggerAction)
                {
                    triggerAction.OnValidate.Invoke(gameObject);
                    OnEnterTriggerAction.Invoke(triggerAction);
                }
            }

            TriggerActionInput();
        }

        protected virtual vTriggerGenericAction GetNearAction()
        {
            if (isLockTriggerEvents || doingAction || playingAnimation)
            {
                return null;
            }

            float distance = Mathf.Infinity;
            vTriggerGenericAction _targetAction = null;

            foreach (var key in actions.Keys)
            {
                if (key)
                {
                    try
                    {
                        vTriggerGenericAction action = actions[key];
                        var screenP = mainCamera ? mainCamera.WorldToScreenPoint(key.transform.position) : screenCenter;
                        if (mainCamera)
                        {

                            bool isValid = action.enabled && action.gameObject.activeInHierarchy && (!action.activeFromForward && (screenP - screenCenter).magnitude < distance || IsInForward(action.transform, action.forwardAngle) && (screenP - screenCenter).magnitude < distance);
                            if (isValid)
                            {
                                distance = (screenP - screenCenter).magnitude;
                                if (_targetAction && _targetAction != action)
                                {
                                    if (actions[_targetAction._collider].isValid)
                                    {
                                        _targetAction.OnInvalidate.Invoke(gameObject);
                                    }

                                    _targetAction = action;
                                }
                                else if (_targetAction == null)
                                {
                                    _targetAction = action;
                                }
                            }
                            else
                            {
                                if (actions[action._collider].isValid)
                                {
                                    action.OnInvalidate.Invoke(gameObject);
                                }

                                OnExitTriggerAction.Invoke(triggerAction);
                            }
                        }
                        else
                        {
                            if (!_targetAction)
                            {
                                _targetAction = action;
                            }
                            else
                            {
                                if (actions[action._collider].isValid)
                                {
                                    action.OnInvalidate.Invoke(gameObject);
                                }
                                OnExitTriggerAction.Invoke(triggerAction);
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                else
                {
                    actions.Remove(key);
                    return null;
                }
            }

            return _targetAction;
        }

        protected virtual bool IsInForward(Transform target, float angleToCompare)
        {
            var angle = Vector3.Angle(transform.forward, target.forward);
            return angle <= angleToCompare;
        }

        protected virtual void AnimationBehaviour()
        {
            if (animationBehaviourDelay > 0 && !playingAnimation)
            {
                animationBehaviourDelay -= Time.deltaTime; return;
            }

            if (playingAnimation)
            {
                // Track root motion delta if snap to animation root is enabled
                if (triggerAction.snapToAnimationRoot && tpInput != null && tpInput.cc != null && tpInput.cc.animator != null)
                {
                    animationRootMotionDelta += tpInput.cc.animator.deltaPosition;
                    
                    // Check if we should snap to root at this point in the animation (for values < 1)
                    float normalizedTime = tpInput.cc.animatorStateInfos.GetCurrentNormalizedTime(triggerAction.animatorLayer);
                    if (triggerAction.snapToRootTime < 1f && normalizedTime >= triggerAction.snapToRootTime && !hasSnappedToRoot)
                    {
                        SnapToAnimationRoot();
                    }
                }
                

                
                if (triggerAction.matchTarget != null)
                {
                    if (debugMode)
                    {
                        Debug.Log($"<b>GenericAction: </b><color=blue>Match Target...</color> ");
                    }

                    EvaluateToTargetPosition();
                }

                if (triggerAction.useTriggerRotation)
                {
                    if (debugMode)
                    {
                        Debug.Log($"<b>GenericAction: </b><color=blue>Rotate to Target...</color> ");
                    }

                    EvaluateToTargetRotation();
                }

                if (actionStarted && !triggerAction.endActionManualy && (triggerAction.inputType != vTriggerGenericAction.InputType.GetButtonTimer || !triggerAction.playAnimationWhileHoldingButton) && tpInput.cc.animatorStateInfos.GetCurrentNormalizedTime(triggerAction.animatorLayer) >= triggerAction.endExitTimeAnimation)
                {
                    if (debugMode)
                    {
                        Debug.Log($"<b>GenericAction: </b>Finish Animation ");
                    }
                    // triggers the OnEndAnimation Event
                    EndAction();
                }
            }
            else if (doingAction && actionStarted && (triggerAction == null || !triggerAction.endActionManualy))
            {
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b>Else if block - doingAction: {doingAction}, actionStarted: {actionStarted}, isWaitingForSnapDelay: {isWaitingForSnapDelay}");
                }
                
                // Handle delay timer countdown when waiting for snap delay (moved from playingAnimation block)
                if (isWaitingForSnapDelay && snapToRootDelayTimer > 0f)
                {
                    float oldTimer = snapToRootDelayTimer;
                    snapToRootDelayTimer -= Time.deltaTime;
                    if (debugMode)
                    {
                        Debug.Log($"<b>GenericAction: </b>Snap delay countdown: {snapToRootDelayTimer:F2}s remaining (was {oldTimer:F2}s), isWaitingForSnapDelay: {isWaitingForSnapDelay}");
                    }
                    if (snapToRootDelayTimer <= 0f)
                    {
                        // Delay finished, snap now
                        if (debugMode)
                        {
                            Debug.Log($"<b>GenericAction: </b>Snap delay finished, snapping to root");
                        }
                        SnapToAnimationRoot();
                        isWaitingForSnapDelay = false;
                        return; // Don't end action yet, let SnapToAnimationRoot handle it
                    }
                    return; // Don't end action yet, wait for delay to finish
                }
                
                // Don't end action if we're waiting for snap delay
                if (isWaitingForSnapDelay)
                {
                    if (debugMode)
                    {
                        Debug.Log($"<b>GenericAction: </b>Skipping EndAction because waiting for snap delay");
                    }
                    return;
                }
                
                //when using a GetButtonTimer the ResetTriggerSettings will be automatically called at the end of the timer or by releasing the input
                if (triggerAction != null && (triggerAction.inputType == vTriggerGenericAction.InputType.GetButtonTimer && triggerAction.playAnimationWhileHoldingButton))
                {
                    return;
                }

                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b>Force ResetTriggerSettings ");
                }
                // triggers the OnEndAnimation Event
                EndAction();
            }
        }

        protected virtual void EvaluateToTargetPosition()
        {
            var matchTargetPosition = triggerAction.matchTarget.position;
            switch (triggerAction.avatarTarget)
            {
                case AvatarTarget.LeftHand:
                    matchTargetPosition = (triggerAction.matchTarget.position - transform.rotation * transform.InverseTransformPoint(tpInput.animator.GetBoneTransform(HumanBodyBones.LeftHand).position));
                    break;
                case AvatarTarget.RightHand:
                    matchTargetPosition = (triggerAction.matchTarget.position - transform.rotation * transform.InverseTransformPoint(tpInput.animator.GetBoneTransform(HumanBodyBones.RightHand).position));
                    break;
                case AvatarTarget.LeftFoot:
                    matchTargetPosition = (triggerAction.matchTarget.position - transform.rotation * transform.InverseTransformPoint(tpInput.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position));
                    break;
                case AvatarTarget.RightFoot:
                    matchTargetPosition = (triggerAction.matchTarget.position - transform.rotation * transform.InverseTransformPoint(tpInput.animator.GetBoneTransform(HumanBodyBones.RightFoot).position));
                    break;
            }
            AnimationCurve XZ = triggerAction.matchPositionXZCurve;
            AnimationCurve Y = triggerAction.matchPositionYCurve;
            float normalizedTime = Mathf.Clamp(tpInput.cc.animatorStateInfos.GetCurrentNormalizedTime(triggerAction.animatorLayer), 0, 1);

            var localRelativeToTarget = triggerAction.matchTarget.InverseTransformPoint(matchTargetPosition);
            if (!triggerAction.useLocalX)
            {
                localRelativeToTarget.x = triggerAction.matchTarget.InverseTransformPoint(transform.position).x;
            }

            if (!triggerAction.useLocalY)
            {
                localRelativeToTarget.y = triggerAction.matchTarget.InverseTransformPoint(transform.position).y;
            }

            if (!triggerAction.useLocalZ)
            {
                localRelativeToTarget.z = triggerAction.matchTarget.InverseTransformPoint(transform.position).z;
            }

            matchTargetPosition = triggerAction.matchTarget.TransformPoint(localRelativeToTarget);

            Vector3 rootPosition = tpInput.cc.animator.rootPosition;

            float evaluatedXZ = XZ.Evaluate(normalizedTime);
            float evaluatedY = Y.Evaluate(normalizedTime);

            if (evaluatedXZ < 1f)
            {
                rootPosition.x = Mathf.Lerp(rootPosition.x, matchTargetPosition.x, evaluatedXZ);
                rootPosition.z = Mathf.Lerp(rootPosition.z, matchTargetPosition.z, evaluatedXZ);
                finishPositionXZMatch = true;
            }
            else if (finishPositionXZMatch)
            {
                finishPositionXZMatch = false;
                rootPosition.x = matchTargetPosition.x;
                rootPosition.z = matchTargetPosition.z;
            }
            if (evaluatedY < 1f)
            {
                rootPosition.y = Mathf.Lerp(rootPosition.y, matchTargetPosition.y, evaluatedY);
                finishPositionYMatch = true;
            }
            else if (finishPositionYMatch)
            {
                finishPositionYMatch = false;
                rootPosition.y = matchTargetPosition.y;
            }

            transform.position = rootPosition;
        }

        protected virtual void EvaluateToTargetRotation()
        {
            var targetEuler = new Vector3(transform.eulerAngles.x, triggerAction.transform.eulerAngles.y, transform.eulerAngles.z);
            Quaternion targetRotation = Quaternion.Euler(targetEuler);
            Quaternion rootRotation = tpInput.cc.animator.rootRotation;
            AnimationCurve rotationCurve = triggerAction.matchRotationCurve;
            float normalizedTime = tpInput.cc.animatorStateInfos.GetCurrentNormalizedTime(triggerAction.animatorLayer);
            float evaluatedCurve = rotationCurve.Evaluate(normalizedTime);
            if (evaluatedCurve < 1)
            {
                rootRotation = Quaternion.Lerp(rootRotation, targetRotation, evaluatedCurve);
                finishRotationMatch = true;
            }
            else if (finishRotationMatch)
            {
                finishRotationMatch = false;
                rootRotation = targetRotation;
            }
            transform.rotation = rootRotation;
        }

        protected virtual void EndAction()
        {
            OnEndAction.Invoke(triggerAction);

            var trigger = triggerAction;
            
            // Handle delay for snapToRootTime = 1
            if (trigger.snapToAnimationRoot && trigger.snapToRootTime >= 1f && !hasSnappedToRoot)
            {
                if (trigger.snapToRootDelay > 0f)
                {
                    // Start the delay timer
                    snapToRootDelayTimer = trigger.snapToRootDelay;
                    isWaitingForSnapDelay = true;
                    if (debugMode)
                    {
                        Debug.Log($"<b>GenericAction: </b>Animation finished, starting snap delay timer: {snapToRootDelayTimer}s, isWaitingForSnapDelay set to: {isWaitingForSnapDelay}");
                    }
                    
                    // Don't end the action yet, wait for delay to finish
                    // triggers the OnEndAnimation Event
                    trigger.OnEndAnimation.Invoke();
                    // Exit the trigger
                    OnExitTriggerAction.Invoke(triggerAction);
                    
                    if (debugMode)
                    {
                        Debug.Log($"<b>GenericAction: </b>Waiting for snap delay to finish...");
                    }
                    return; // Exit early, don't reset settings yet
                }
                else
                {
                    // No delay, snap immediately
                    SnapToAnimationRoot();
                }
            }
            else if (trigger.snapToAnimationRoot && !hasSnappedToRoot)
            {
                // For other snapToRootTime values, snap immediately
                SnapToAnimationRoot();
            }
            
            // triggers the OnEndAnimation Event
            trigger.OnEndAnimation.Invoke();
            // Exit the trigger
            OnExitTriggerAction.Invoke(triggerAction);
            // reset GenericAction variables so you can use it again
            ResetTriggerSettings();

            // Destroy trigger after reset all settings
            if (trigger.destroyAfter)
            {
                StartCoroutine(DestroyActionDelay(trigger));
            }

            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>End Action ");
            }
        }

        protected virtual void SnapToAnimationRoot()
        {
            if (triggerAction == null || !triggerAction.snapToAnimationRoot)
                return;

            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>Snapping to animation root at {triggerAction.snapToRootTime * 100}% of animation...");
            }
            
            Vector3 oldPosition = transform.position;
            Vector3 finalPosition;
            
            // Use player model position if available, otherwise fall back to root motion calculation
            if (triggerAction.playerModel != null)
            {
                finalPosition = triggerAction.playerModel.position;
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b>Using player model position: {finalPosition} (was: {oldPosition})");
                }
            }
            else
            {
                finalPosition = animationStartPosition + animationRootMotionDelta;
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b>Using calculated position: {finalPosition} (start: {animationStartPosition}, delta: {animationRootMotionDelta}, was: {oldPosition})");
                }
            }
            
            transform.position = finalPosition;
            hasSnappedToRoot = true;
            
            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>Snapped to position: {finalPosition} (was: {oldPosition})");
            }
            
            // If we were waiting for delay, now finish the action
            if (isWaitingForSnapDelay)
            {
                FinishDelayedAction();
            }
        }
        
        protected virtual void FinishDelayedAction()
        {
            var trigger = triggerAction;
            
            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>Finishing delayed action...");
            }
            
            // reset GenericAction variables so you can use it again
            ResetTriggerSettings();

            // Destroy trigger after reset all settings
            if (trigger.destroyAfter)
            {
                StartCoroutine(DestroyActionDelay(trigger));
            }

            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>Delayed action finished");
            }
        }

        public virtual bool playingAnimation
        {
            get
            {
                if (triggerAction == null || !doingAction)
                {
                    return _playingAnimation = false;
                }

                if (!_playingAnimation && inActionAnimation)
                {
                    _playingAnimation = true;
                    triggerAction.OnStartAnimation.Invoke();
                    DisablePlayerGravityAndCollision();
                }
                else if (_playingAnimation && !inActionAnimation)
                {
                    _playingAnimation = false;
                }
                return _playingAnimation;
            }
            protected set
            {
                _playingAnimation = true;
            }
        }

        public virtual bool actionConditions
        {
            get
            {
                return (!doingAction && !playingAnimation && !tpInput.cc.isJumping && !tpInput.cc.customAction /*&& !tpInput.cc.animator.IsInTransition(triggerAction.animatorLayer)*/);
            }
        }

        public override void OnActionEnter(Collider other)
        {
            if (isLockTriggerEvents)
            {
                return;
            }

            if (other != null && other.gameObject.CompareTag(actionTag))
            {
                if (!actions.ContainsKey(other))
                {
                    vTriggerGenericAction[] _triggerActions = other.GetComponents<vTriggerGenericAction>();
                    for (int i = 0; i < _triggerActions.Length; i++)
                    {
                        var _triggerAction = _triggerActions[i];
                        if (_triggerAction && _triggerAction.enabled)
                        {
                            actions.Add(other, _triggerAction);
                            _triggerAction.OnPlayerEnter.Invoke(gameObject);
                            if (debugMode)
                            {
                                Debug.Log("<color=green>Enter in Trigger </color>" + other.gameObject, other.gameObject);
                            }
                            break;
                        }
                    }

                }
            }
        }

        public override void OnActionExit(Collider other)
        {
            if (isLockTriggerEvents)
            {
                return;
            }

            if (other.gameObject.CompareTag(actionTag) && actions.ContainsKey(other) && (!doingAction || other != triggerAction._collider))
            {
                vTriggerGenericAction action = actions[other];
                actions.Remove(other);
                action.OnPlayerExit.Invoke(gameObject);
                action.OnInvalidate.Invoke(gameObject);
                OnExitTriggerAction.Invoke(action);
                if (debugMode)
                {
                    Debug.Log("<color=red>Exit of Trigger </color> " + other.gameObject, other.gameObject);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (isLockTriggerEvents)
            {
                return;
            }

            if (other.gameObject.CompareTag(actionTag) && actions.ContainsKey(other) && (!doingAction || other != triggerAction._collider))
            {
                vTriggerGenericAction action = actions[other];
                actions.Remove(other);
                action.OnPlayerExit.Invoke(gameObject);
                action.OnInvalidate.Invoke(gameObject);
                OnExitTriggerAction.Invoke(action);
                if (debugMode)
                {
                    Debug.Log("<color=red>Exit of Trigger </color> " + other.gameObject, other.gameObject);
                }
            }
        }

        public override void OnActionStay(Collider other)
        {
            if (isLockTriggerEvents)
            {
                return;
            }

            if (other != null && actions.ContainsKey(other))
            {

                actions[other].action.OnPlayerStay.Invoke(gameObject);
                timeInTrigger = .5f;
                if (debugMode)
                {
                    Debug.Log("<color=yellow>Stay in Trigger </color>" + other.gameObject, other.gameObject);
                }
            }
        }

        /// <summary>
        /// End Action Manualy if <see cref="vTriggerGenericAction.endActionManualy"/> equals true
        /// </summary>
        public virtual void FinishAction()
        {
            if (triggerAction && actionStarted && triggerAction.endActionManualy)
            {
                EndAction();
            }
        }

        public virtual void CancelAction()
        {
            if (triggerAction && actionStarted)
            {
                var trigger = triggerAction;
                // triggers the OnEndAnimation Event
                trigger.OnCancelAction.Invoke();
                // Exit the trigger
                OnExitTriggerAction.Invoke(triggerAction);
                // reset GenericAction variables so you can use it again
                ResetTriggerSettings();
                // Destroy trigger after reset all settings
                if (trigger.destroyAfter)
                {
                    StartCoroutine(DestroyActionDelay(trigger));
                }
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b>Cancel Action ");
                }
            }
        }

        public virtual void TriggerActionInput()
        {
            if (triggerAction == null || !triggerAction.gameObject.activeInHierarchy || (triggerAction.CanDoAction == false))
            {
                return;
            }

            // AutoAction
            if (triggerAction.inputType == vTriggerGenericAction.InputType.AutoAction && actionConditions)
            {
                TriggerActionEvents();
                TriggerAnimation();
            }
            // GetButtonDown
            else if (triggerAction.inputType == vTriggerGenericAction.InputType.GetButtonDown && actionConditions)
            {
                if (triggerAction.actionInput.GetButtonDown())
                {
                    TriggerActionEvents();
                    TriggerAnimation();
                }
            }
            // GetDoubleButton
            else if (triggerAction.inputType == vTriggerGenericAction.InputType.GetDoubleButton && actionConditions)
            {
                if (triggerAction.actionInput.GetDoubleButtonDown(triggerAction.doubleButtomTime))
                {
                    TriggerActionEvents();
                    TriggerAnimation();
                }
            }
            // GetButtonTimer (Hold Button)
            else if (triggerAction.inputType == vTriggerGenericAction.InputType.GetButtonTimer)
            {
                if (_currentInputDelay <= 0)
                {
                    var up = false;
                    var t = 0f;

                    // this mode will play the animation while you're holding the button
                    if (triggerAction.playAnimationWhileHoldingButton)
                    {
                        TriggerActionEventsInput();

                        // call the OnFinishActionInput after the buttomTimer is concluded and reset player settings
                        if (triggerAction.actionInput.GetButtonTimer(ref t, ref up, triggerAction.buttonTimer))
                        {
                            if (debugMode)
                            {
                                Debug.Log($"<b>GenericAction: </b>Finish Action Input ");
                            }

                            triggerAction.UpdateButtonTimer(0);
                            triggerAction.OnFinishActionInput.Invoke();

                            ResetActionState();
                            EndAction();
                            //ResetTriggerSettings();
                        }

                        // trigger the Animation and the ActionEvents while your hold the button
                        if (triggerAction && triggerAction.actionInput.inButtomTimer)
                        {
                            if (debugMode)
                            {
                                Debug.Log($"<b>GenericAction: </b><color=blue>Holding Input</color>  ");
                            }

                            triggerAction.UpdateButtonTimer(t);
                            TriggerAnimation();
                        }

                        // call OnCancelActionInput if the button is released before ending the buttonTimer
                        if (up && triggerAction)
                        {
                            CancelButtonTimer();
                        }
                    }
                    // this mode will play the animation after you finish holding the button
                    else /*if (!doingAction)*/
                    {
                        TriggerActionEventsInput();

                        // call the OnFinishActionInput after the buttomTimer is concluded and reset player settings
                        if (triggerAction.actionInput.GetButtonTimer(ref t, ref up, triggerAction.buttonTimer))
                        {
                            if (debugMode)
                            {
                                Debug.Log($"<b>GenericAction: </b>Finish Action Input ");
                            }

                            triggerAction.UpdateButtonTimer(0);
                            triggerAction.OnFinishActionInput.Invoke();
                            // destroy the triggerAction if checked with destroyAfter                          
                            TriggerAnimation();
                        }

                        // trigger the ActionEvents while your hold the button
                        if (triggerAction && triggerAction.actionInput.inButtomTimer)
                        {
                            if (debugMode)
                            {
                                Debug.Log($"<b>GenericAction: </b><color=blue>Holding Input</color>");
                            }

                            triggerAction.UpdateButtonTimer(t);
                        }

                        // call OnCancelActionInput if the button is released before ending the buttonTimer
                        if (up && triggerAction)
                        {
                            CancelButtonTimer();
                        }
                    }
                }
                else
                {
                    _currentInputDelay -= Time.deltaTime;
                }
            }
        }

        protected virtual void CancelButtonTimer()
        {
            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>Cancel Action ");
            }

            triggerAction.OnCancelActionInput.Invoke();
            _currentInputDelay = triggerAction.inputDelay;
            triggerAction.UpdateButtonTimer(0);
            OnCancelAction.Invoke(triggerAction);
            ResetActionState();
            ResetTriggerSettings(false);
        }

        protected virtual void TriggerActionEventsInput()
        {
            // trigger the ActionEvents while your hold the button
            if (triggerAction && triggerAction.actionInput.GetButtonDown())
            {
                TriggerActionEvents();
            }
        }

        public virtual void TriggerActionEvents()
        {
            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>TriggerAction Events ", gameObject);
            }

            doingAction = true;
            // Call OnStartAction from the Controller's GenericAction inspector
            OnStartAction.Invoke(triggerAction);

            // Call OnDoAction from the Controller's GenericAction
            OnDoAction.Invoke(triggerAction);

            // trigger OnDoAction Event, you can add a delay in the inspector
            StartCoroutine(triggerAction.OnPressActionDelay(gameObject));
        }

        public virtual void TriggerAnimation()
        {
            if (playingAnimation || actionStarted)
            {
                return;
            }

            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>TriggerAnimation ", gameObject);
            }

            if (triggerAction.animatorActionState != 0)
            {
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b>Applied ActionState: " + triggerAction.animatorActionState + " ", gameObject);
                }

                tpInput.cc.SetActionState(triggerAction.animatorActionState);
            }

            // trigger the animation behaviour & match target
            if (!string.IsNullOrEmpty(triggerAction.playAnimation))
            {
                if (!actionStarted)
                {
                    if (debugMode)
                    {
                        Debug.Log($"<b>GenericAction: </b>PlayAnimation: " + triggerAction.playAnimation + " ", gameObject);
                    }

                    actionStarted = true;
                    playingAnimation = true;
                    
                    // Capture the start position if we need to snap to animation root
                    if (triggerAction.snapToAnimationRoot)
                    {
                        animationStartPosition = transform.position;
                        animationRootMotionDelta = Vector3.zero; // Reset the accumulated delta
                        hasSnappedToRoot = false; // Reset the snap flag
                        snapToRootDelayTimer = 0f; // Reset the delay timer
                        isWaitingForSnapDelay = false; // Reset the delay waiting flag
                        
                        // Capture player model start position if available
                        if (triggerAction.playerModel != null)
                        {
                            playerModelStartPosition = triggerAction.playerModel.position;
                            if (debugMode)
                            {
                                Debug.Log($"<b>GenericAction: </b>Captured player model start position: {playerModelStartPosition}");
                            }
                        }
                        
                        if (debugMode)
                        {
                            Debug.Log($"<b>GenericAction: </b>Captured animation start position: {animationStartPosition}");
                        }
                    }
                    
                    tpInput.cc.animator.CrossFadeInFixedTime(triggerAction.playAnimation, triggerAction.crossFadeTransition, triggerAction.animatorLayer);    // trigger the action animation clip
                    if (!string.IsNullOrEmpty(triggerAction.customCameraState))
                    {
                        tpInput.ChangeCameraState(triggerAction.customCameraState, true);           // change current camera state to a custom
                    }
                }
                animationBehaviourDelay = triggerAction.crossFadeTransition + 0.1f;
            }
            else
            {
                actionStarted = true;
            }
        }

        public virtual void ResetActionState()
        {
            if (triggerAction && triggerAction.resetAnimatorActionState)
            {
                tpInput.cc.SetActionState(0);
            }
        }

        public virtual void ResetTriggerSettings(bool removeTrigger = true)
        {
            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>Reset Trigger Settings ");
            }

            // reset player gravity and collision
            EnablePlayerGravityAndCollision();
            // reset the Animator parameter ActionState back to 0 
            ResetActionState();
            // reset the CameraState to the Default state
            if (triggerAction != null && !string.IsNullOrEmpty(triggerAction.customCameraState))
            {
                tpInput.ResetCameraState();
            }
            // remove the collider from the actions list
            if (triggerAction != null && actions.ContainsKey(triggerAction._collider) && removeTrigger)
            {
                actions.Remove(triggerAction._collider);
            }
            triggerAction = null;
            doingAction = false;
            actionStarted = false;
            if (debugMode && isWaitingForSnapDelay)
            {
                Debug.Log($"<b>GenericAction: </b>Resetting isWaitingForSnapDelay from true to false in ResetTriggerSettings");
            }
            isWaitingForSnapDelay = false; // Reset the delay waiting flag
        }

        public virtual void DisablePlayerGravityAndCollision()
        {
            if (triggerAction && triggerAction.disableGravity)
            {
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b><color=red>Disable Player's Gravity</color> ");
                }

                tpInput.cc._rigidbody.useGravity = false;
                if (!tpInput.cc._rigidbody.isKinematic)
                    tpInput.cc._rigidbody.velocity = Vector3.zero;
                tpInput.cc._rigidbody.isKinematic = true;
            }
            if (triggerAction && triggerAction.disableCollision)
            {
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b><color=red>Disable Player's Collision</color> ");
                }

                tpInput.cc._capsuleCollider.isTrigger = true;
            }
        }

        public virtual void EnablePlayerGravityAndCollision()
        {
            if (triggerAction && triggerAction.disableGravity)
            {
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b><color=red>Enable Player's Gravity</color> ");
                }

                tpInput.cc._rigidbody.useGravity = true;
                tpInput.cc._rigidbody.isKinematic = false;
            }
            if (triggerAction && triggerAction.disableCollision)
            {
                if (debugMode)
                {
                    Debug.Log($"<b>GenericAction: </b><color=red>Enable Player's Collision</color> ");
                }

                tpInput.cc._capsuleCollider.isTrigger = false;
            }
        }

        public virtual IEnumerator DestroyActionDelay(vTriggerGenericAction triggerAction)
        {
            var _triggerAction = triggerAction;
            yield return new WaitForSeconds(_triggerAction.destroyDelay);
            if (_triggerAction != null && _triggerAction.gameObject != null)
            {
                OnExitTriggerAction.Invoke(triggerAction);
                Destroy(_triggerAction.gameObject);
            }

            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>Destroy Trigger ");
            }
        }

        public virtual void SetLockTriggerEvents(bool value)
        {
            foreach (var key in actions.Keys)
            {
                if (key)
                {
                    actions[key].action.OnPlayerExit.Invoke(gameObject);
                    actions[key].action.OnInvalidate.Invoke(gameObject);
                }
            }
            actions.Clear();
            isLockTriggerEvents = value;
        }
    }
}