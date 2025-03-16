#if INVECTOR_BASIC
using Invector;
#endif
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static com.mobilin.games.MISAnimator;
using System.IO;
using System.Collections.Generic;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public partial class MISMainSetup
    {
#if MIS_WALLRUN
        // ----------------------------------------------------------------------------------------------------
        // Animator StateMachine/State
        public const string STATE_WALLRUN = MISEditorTagLayer.TAG_WALLRUN;

        // Base Layer
        AnimatorStateMachine base_WallRunSM;
        AnimatorState base_WallRunSM_StartL;
        AnimatorState base_WallRunSM_RunL;
        AnimatorState base_WallRunSM_StartR;
        AnimatorState base_WallRunSM_RunR;
        AnimatorState base_WallRunSM_Jump;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void WallRunSetup(mvSetupOption setupOption, GameObject characterObj, GameObject cameraObj)
        {
            // ----------------------------------------------------------------------------------------------------
            // Setup Options
            // ----------------------------------------------------------------------------------------------------


            // ----------------------------------------------------------------------------------------------------
            // Main Component
            // ----------------------------------------------------------------------------------------------------
            mvWallRun package = characterObj.GetComponent<mvWallRun>();

            if (package == null)
                package = characterObj.AddComponent<mvWallRun>();


            // ----------------------------------------------------------------------------------------------------
            // Animator
            // ----------------------------------------------------------------------------------------------------
            WallRunAnimatorParameters();
            WallRunBaseLayer();
            WallRunAnimatorTransitions();
            WallRunPosition();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void WallRunAnimatorParameters()
        {
            // WallRunState
            if (!animatorController.parameters.HasParameter(PARAM_WALLRUN_STATE))
                animatorController.AddParameter(PARAM_WALLRUN_STATE, AnimatorControllerParameterType.Int);


            // AnimationSpeed
            if (!animatorController.parameters.HasParameter(PARAM_ANIMATION_SPEED))
                animatorController.AddParameter(PARAM_ANIMATION_SPEED, AnimatorControllerParameterType.Float);
            animatorController.SetParameter(PARAM_ANIMATION_SPEED, 1f);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void WallRunBaseLayer()
        {
            // ----------------------------------------------------------------------------------------------------
            // Animation Clips
            // ----------------------------------------------------------------------------------------------------
            AnimationClip wallRunStartLMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_WALLRUN_PATH, "Runtime/Animations/WallRun@RunStart_L.anim"));
            AnimationClip wallRunStartRMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_WALLRUN_PATH, "Runtime/Animations/WallRun@RunStart_R.anim"));
            AnimationClip wallRunLMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_WALLRUN_PATH, "Runtime/Animations/WallRun@Run_L.anim"));
            AnimationClip wallRunRMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_WALLRUN_PATH, "Runtime/Animations/WallRun@Run_R.anim"));
            AnimationClip wallJumpMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_WALLRUN_PATH, "Runtime/Animations/WallRun@Jump_TwistFlip.anim"));


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion
            // ----------------------------------------------------------------------------------------------------

            // MIS
            base_Locomotion_MIS = base_LocomotionSM.CreateStateMachineIfNotExist(STATE_MIS);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion - WallRunSM
            // ----------------------------------------------------------------------------------------------------
            base_WallRunSM = base_Locomotion_MIS.CreateStateMachineIfNotExist(STATE_WALLRUN);


            // ----------------------------------------------------------------------------------------------------
            // WallRun StartL AnimatorState
            base_WallRunSM_StartL = base_WallRunSM.CreateStateIfNotExist("StartL", wallRunStartLMotion);


            // ----------------------------------------------------------------------------------------------------
            // WallRun StartR AnimatorState
            base_WallRunSM_StartR = base_WallRunSM.CreateStateIfNotExist("StartR", wallRunStartRMotion);


            // ----------------------------------------------------------------------------------------------------
            // WallRun RunL AnimatorState
            base_WallRunSM_RunL = base_WallRunSM.CreateStateIfNotExist("RunL", wallRunLMotion);


            // ----------------------------------------------------------------------------------------------------
            // WallRun RunR AnimatorState
            base_WallRunSM_RunR = base_WallRunSM.CreateStateIfNotExist("RunR", wallRunRMotion);


            // ----------------------------------------------------------------------------------------------------
            // WallRun WallJump AnimatorState
            base_WallRunSM_Jump = base_WallRunSM.CreateStateIfNotExist("WallJump", wallJumpMotion);
            base_WallRunSM_Jump.speed = 1f;
            base_WallRunSM_Jump.speedParameter = PARAM_ANIMATION_SPEED;
            base_WallRunSM_Jump.speedParameterActive = true;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void WallRunAnimatorTransitions()
        {
            // ----------------------------------------------------------------------------------------------------
            // Base Layer
            // ----------------------------------------------------------------------------------------------------
            conditionList.Clear();
            misConditionList.Clear();

            // ----------------------------------------------------------------------------------------------------
            // Base - Airborne - Falling
            // ----------------------------------------------------------------------------------------------------

            // Any To Falling
            List<AnimatorStateTransition> allAny2FallingTransitions = base_Root.FindAllAnyTransition(base_FallingSM_Falling);

            for (int i = 0; i < allAny2FallingTransitions.Count; i++)
            {
                if (!allAny2FallingTransitions[i].HasCondition(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None)))
                    allAny2FallingTransitions[i].AddCondition(AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None, PARAM_WALLRUN_STATE);
            }

            // Falling To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.NotEqual, (int)mvWallRun.WallRunStateType.None));
            base_FallingSM_Falling.AddExitTransitionIfNotExist(conditionList, false);

            // Any To Sliding
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_SLIDING, AnimatorConditionMode.If, 0f));  // true
            var any2Sliding = base_Root.FindAnyTransitionIfContains(base_FallingSM_Sliding, conditionList);
            if (any2Sliding == null)
                any2Sliding = base_Root.AddAnyTransition(base_FallingSM_Sliding, conditionList, false, 0f, true, 0.25f);
            if (!any2Sliding.HasCondition(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None)))
                any2Sliding.AddCondition(AnimatorConditionMode.Equals, 0, PARAM_WALLRUN_STATE);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion
            // ----------------------------------------------------------------------------------------------------

            // ----------------------------------------------------------------------------------------------------
            // MIS

            // MIS to Exit
            base_LocomotionSM.AddExitTransitionIfNotExist(base_Locomotion_MIS, null);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion - MIS
            // ----------------------------------------------------------------------------------------------------

            // ----------------------------------------------------------------------------------------------------
            // WallRun

            // Entry To WallRun
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.NotEqual, (int)mvWallRun.WallRunStateType.None));
            base_LocomotionSM.AddEntryTransitionIfNotExist(base_WallRunSM, conditionList);

            // WallRun to Exit
            base_Locomotion_MIS.AddExitTransitionIfNotExist(base_WallRunSM, null);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion - MIS - WallRun
            // ----------------------------------------------------------------------------------------------------

            // StartL To RunL
            base_WallRunSM_StartL.AddTransitionIfNotExist(base_WallRunSM_RunL, null, true, 0.7f, true, 0.3f);

            // RunL To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None));
            base_WallRunSM_RunL.AddExitTransitionIfNotExist(conditionList, false);

            // RunL To WallJump
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.Jump));
            base_WallRunSM_RunL.AddTransitionIfNotExist(base_WallRunSM_Jump, conditionList, false, 0f, true, 0.2f);

            // WallJump To RunL
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunL));
            base_WallRunSM_Jump.AddTransitionIfNotExist(base_WallRunSM_RunL, conditionList, false);

            // StartR To RunR
            base_WallRunSM_StartR.AddTransitionIfNotExist(base_WallRunSM_RunR, null, true, 0.7f, true, 0.3f);

            // RunR To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None));
            base_WallRunSM_RunR.AddExitTransitionIfNotExist(conditionList, false);

            // RunR To WallJump
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.Jump));
            base_WallRunSM_RunR.AddTransitionIfNotExist(base_WallRunSM_Jump, conditionList, false, 0f, true, 0.2f);

            // WallJump To RunR
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunR));
            base_WallRunSM_Jump.AddTransitionIfNotExist(base_WallRunSM_RunR, conditionList, false);

            // WallJump To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None));
            base_WallRunSM_Jump.AddExitTransitionIfNotExist(conditionList, false);


            // RunL To RunR
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunR));
            base_WallRunSM_RunL.AddTransitionIfNotExist(base_WallRunSM_RunR, conditionList, false, 0f, true, 0.2f);

            // RunR To RunL
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunL));
            base_WallRunSM_RunR.AddTransitionIfNotExist(base_WallRunSM_RunL, conditionList, false, 0f, true, 0.2f);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion - Free Locomotion
            // ----------------------------------------------------------------------------------------------------

            // Entry To Free Movement
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_CROUCHING, AnimatorConditionMode.IfNot, 0f));  // false
            misConditionList.Clear();
            misConditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None));
            base_LocomotionSM.SetEntryTransition(base_FreeMovement, conditionList, misConditionList);

            // Entry To Free Crouch
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_CROUCHING, AnimatorConditionMode.If, 0f));  // true
            misConditionList.Clear();
            misConditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None));
            base_LocomotionSM.SetEntryTransition(base_FreeCrouch, conditionList, misConditionList);

            // Free Movement To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.NotEqual, (int)mvWallRun.WallRunStateType.None));
            base_FreeMovement.AddExitTransitionIfNotExist(conditionList, false, 0f, true, 0.2f);

            // Free Crouch To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f));   // true
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.NotEqual, (int)mvWallRun.WallRunStateType.None));
            var freeCrouchToExit = base_FreeCrouch.FindSameExitTransition(conditionList);

            if (freeCrouchToExit == null)
            {
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f));   // true
                freeCrouchToExit = base_FreeCrouch.FindSameExitTransition(conditionList);

                if (freeCrouchToExit == null)
                {
                    conditionList.Clear();
                    conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f)); // true
                    conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.NotEqual, (int)mvWallRun.WallRunStateType.None));
                    base_FreeCrouch.AddExitTransition(conditionList, false, 0.87f);
                }
                else
                {
                    freeCrouchToExit.AddCondition(AnimatorConditionMode.NotEqual, (int)mvWallRun.WallRunStateType.None, PARAM_WALLRUN_STATE);
                }
            }


            // Free Locomotion To StartL
            conditionList.Clear();
            //conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Less, 0.1f));
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunL));
            base_LocomotionSM.AddTransitionIfNotExist(base_FreeLocomotionSM, base_WallRunSM_StartL, conditionList);

            // Free Locomotion To StartR
            conditionList.Clear();
            //conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Less, 0.1f));
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunR));
            base_LocomotionSM.AddTransitionIfNotExist(base_FreeLocomotionSM, base_WallRunSM_StartR, conditionList);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion - Strafe Locomotion
            // ----------------------------------------------------------------------------------------------------

            // Entry To Strafing Movement
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f));  // true
            conditionList.Add(Condition(PARAM_IS_CROUCHING, AnimatorConditionMode.IfNot, 0f));  // false
            misConditionList.Clear();
            misConditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None));
            base_LocomotionSM.SetEntryTransition(base_StrafingMovement, conditionList, misConditionList);

            // Entry To Strafing Crouch
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f));  // true
            conditionList.Add(Condition(PARAM_IS_CROUCHING, AnimatorConditionMode.If, 0f));  // true
            misConditionList.Clear();
            misConditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.None));
            base_LocomotionSM.SetEntryTransition(base_StrafingCrouch, conditionList, misConditionList);

            // Strafing Movement To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.NotEqual, (int)mvWallRun.WallRunStateType.None));
            AnimatorStateTransition strafingMovementToExit = base_StrafingMovement.FindSameExitTransition(conditionList);
            if (strafingMovementToExit == null)
                strafingMovementToExit = base_StrafingMovement.AddExitTransitionIfNotExist(conditionList);

            // Strafing Crouch To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.NotEqual, (int)mvWallRun.WallRunStateType.None));
            AnimatorStateTransition strafingCrouchToExit = base_StrafingCrouch.FindSameExitTransition(conditionList);
            if (strafingCrouchToExit == null)
                strafingCrouchToExit = base_StrafingCrouch.AddExitTransitionIfNotExist(conditionList);


            // Strafing Locomotion To StartL
            conditionList.Clear();
            //conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Less, 0.1f));
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunL));
            var strafeLocomotion2WallRunStartL = base_LocomotionSM.FindSameTransition(base_StrafeLocomotionSM, base_WallRunSM_StartL, conditionList);

            if (strafeLocomotion2WallRunStartL == null)
            {
                conditionList.Clear();
                //conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Greater, 0.25f));
                conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunL));
                base_LocomotionSM.AddTransition(base_StrafeLocomotionSM, base_WallRunSM_StartL, conditionList);
            }

            // Strafing Locomotion To StartR
            conditionList.Clear();
            //conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Less, 0.1f));
            conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunR));
            var strafeLocomotion2WallRunStartR = base_LocomotionSM.FindSameTransition(base_StrafeLocomotionSM, base_WallRunSM_StartR, conditionList);

            if (strafeLocomotion2WallRunStartR == null)
            {
                conditionList.Clear();
                //conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Greater, 0.25f));
                conditionList.Add(Condition(PARAM_WALLRUN_STATE, AnimatorConditionMode.Equals, (int)mvWallRun.WallRunStateType.RunR));
                base_LocomotionSM.AddTransition(base_StrafeLocomotionSM, base_WallRunSM_StartR, conditionList);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void WallRunPosition()
        {
            // ----------------------------------------------------------------------------------------------------
            // Base Layer
            // ----------------------------------------------------------------------------------------------------


            // ----------------------------------------------------------------------------------------------------
            // Locomotion

            // MIS
            base_LocomotionSM.SetStateMachinePosition(base_Locomotion_MIS, BASE_LOCOMOTION_MIS_POS);
            base_Locomotion_MIS.SetDefaultLayerAllPosition();
            base_Locomotion_MIS.ArrangeStatemachines(0);


            // ----------------------------------------------------------------------------------------------------
            // Locomotion - MIS

            // WallRun
            base_WallRunSM.SetDefaultLayerAllPosition();


            // ----------------------------------------------------------------------------------------------------
            // Locomotion - MIS - WallRun

            base_WallRunSM.SetStateRelativePosition(base_WallRunSM_RunL, 3, -1);
            base_WallRunSM.SetStateRelativePosition(base_WallRunSM_StartL, 3, -2);

            base_WallRunSM.SetStateRelativePosition(base_WallRunSM_RunR, 3, 1);
            base_WallRunSM.SetStateRelativePosition(base_WallRunSM_StartR, 3, 2);

            base_WallRunSM.SetStateRelativePosition(base_WallRunSM_Jump, 0, 0);
        }
#endif
    }
}