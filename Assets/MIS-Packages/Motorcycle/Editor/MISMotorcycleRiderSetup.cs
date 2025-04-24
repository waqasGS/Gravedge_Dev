#if INVECTOR_BASIC
using Invector;
using Invector.vEventSystems;
#endif
#if INVECTOR_MELEE
using Invector.vMelee;
#endif
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static com.mobilin.games.MISAnimator;
using UnityEngine.Events;
using UnityEditor.Events;
using System.IO;
using Invector.vCamera;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public partial class MISMainSetup
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        bool useVolumetricHeadLight;
        bool useMaleVoice;

        // ----------------------------------------------------------------------------------------------------
        // Animator StateMachine/State
        public const string STATE_MOTORCYCLE = "Motorcycle";

        // Base Layer
        AnimatorStateMachine base_MotorcycleSM;

        AnimatorState base_Riding;
        AnimatorState base_DummyIdle;

        AnimatorStateMachine base_GetOnSM;
        AnimatorState base_GetOnMotorcycleRFar;
        AnimatorState base_GetOnMotorcycleLFar;
        AnimatorState base_GetOnMotorcycleRNear;
        AnimatorState base_GetOnMotorcycleLNear;
        AnimatorState base_GetOnMotorcycleDummy;

        AnimatorStateMachine base_GetOffSM;
        AnimatorState base_GetOffMotorcycleR;
        AnimatorState base_GetOffMotorcycleL;
        AnimatorState base_GetOffMotorcycleDummy;

#if INVECTOR_SHOOTER
        // UpperBody Layer
        AnimatorStateMachine upb_MotorcycleAimingSM;
        AnimatorState upb_MotorcycleAimingSM_AimPose;
        AnimatorState upb_MotorcycleAimingSM_CantAim;
#endif


        // ----------------------------------------------------------------------------------------------------
        // - Vehicle 레이어 추가해야 한다.
        // - 셋업 윈도우 백그라운드에 셋업이 필요한 오토바이 부속 그림을 배경으로 깔고 해당 부품 오브젝트를 드래그&드롭하면
        //   머티리얼을 렌더링해서 보여주면 좋겠다.
        //   바디, 앞바퀴/뒷바퀴, 스티어링휠
        // ----------------------------------------------------------------------------------------------------
        void MotorcycleRiderSetup(mvSetupOption setupOption, GameObject characterObj, GameObject cameraObj)
        {
            // ----------------------------------------------------------------------------------------------------
            // Setup Options
            // ----------------------------------------------------------------------------------------------------
            useVolumetricHeadLight = setupOption.HasSetupOption(SetupOption.VolumetricHeadlight);
            useMaleVoice = setupOption.HasSetupOption(SetupOption.MaleVoice);


            // ----------------------------------------------------------------------------------------------------
            // Main Component
            // ----------------------------------------------------------------------------------------------------
            mvMotorcycleRider package = null;
            if (templateType == MISEditor.TemplateType.Shooter)
            {
                package = characterObj.GetComponent<mvMotorcycleRiderShooter>();
                if (package == null)
                    package = characterObj.AddComponent<mvMotorcycleRiderShooter>();
            }
            else if (templateType == MISEditor.TemplateType.Melee)
            {
                package = characterObj.GetComponent<mvMotorcycleRiderMelee>();
                if (package == null)
                    package = characterObj.AddComponent<mvMotorcycleRiderMelee>();
            }
            else
            {
                package = characterObj.GetComponent<mvMotorcycleRiderBasic>();
                if (package == null)
                    package = characterObj.AddComponent<mvMotorcycleRiderBasic>();
            }


            // ----------------------------------------------------------------------------------------------------
            // Vehicle Status UI
            misUIParent = misComponentsParentObj.transform.Find("UI");
            if (misUIParent == null)
            {
                GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISEditor.MIS_ASSETS_PATH, "MIS/Prefabs/UI.prefab"));
                misUIParent = uiPrefab.Instantiate3D(Vector3.zero, misComponentsParentObj.transform).transform;
            }

            mvVehicleStatus vehicleStatus = misUIParent.GetComponent<mvVehicleStatus>();
            if (vehicleStatus == null)
                vehicleStatus = misUIParent.gameObject.AddComponent<mvVehicleStatus>();

            if (!misUIParent.Find("MotorcycleIcon"))
            {
                GameObject motorcycleIconPrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/MotorcycleIcon.prefab"));
                vehicleStatus.vehicleIconObj = motorcycleIconPrefab.Instantiate2D(Vector3.zero, misUIParent);
            }

            if (package.OnFinishGetOn == null)
                package.OnFinishGetOn = new UnityEvent();

            package.OnFinishGetOn.RemoveMissingPersistents();
            if (package.OnFinishGetOn.HasPersistent(vehicleStatus, vehicleStatus.GetType(), "SetStatus", typeof(int)) == false)
            {
                UnityAction<int> setStatusOnDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<int>), vehicleStatus, "SetStatus") as UnityAction<int>;
                UnityEventTools.AddIntPersistentListener(package.OnFinishGetOn, setStatusOnDelegate, 1);
            }

            if (package.OnFailGetOn == null)
                package.OnFailGetOn = new UnityEvent();

            package.OnFailGetOn.RemoveMissingPersistents();
            if (package.OnFailGetOn.HasPersistent(vehicleStatus, vehicleStatus.GetType(), "SetStatus", typeof(int)) == false)
            {
                UnityAction<int> setStatusDisabledDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<int>), vehicleStatus, "SetStatus") as UnityAction<int>;
                UnityEventTools.AddIntPersistentListener(package.OnFailGetOn, setStatusDisabledDelegate, 2);
            }

            if (package.OnFinishGetOff == null)
                package.OnFinishGetOff = new UnityEvent();

            package.OnFinishGetOff.RemoveMissingPersistents();
            if (package.OnFinishGetOff.HasPersistent(vehicleStatus, vehicleStatus.GetType(), "SetStatus", typeof(int)) == false)
            {
                UnityAction<int> setStatusNoneDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<int>), vehicleStatus, "SetStatus") as UnityAction<int>;
                UnityEventTools.AddIntPersistentListener(package.OnFinishGetOff, setStatusNoneDelegate, 0);
            }


            // ----------------------------------------------------------------------------------------------------
            // RiderAudioSource
            GameObject riderAudioSourceInstance = null;
            if (misComponentsParentObj.transform.Find("RiderSource") == null)
            {
                GameObject riderAudioSourcePrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/RiderSource.prefab"));
                riderAudioSourceInstance = Instantiate(riderAudioSourcePrefab, Vector3.zero, Quaternion.identity, misComponentsParentObj.transform);
                riderAudioSourceInstance.name = "RiderSource";
                riderAudioSourceInstance.transform.localPosition = Vector3.zero;
                riderAudioSourceInstance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                riderAudioSourceInstance = misComponentsParentObj.transform.Find("RiderSource").gameObject;
            }

            package.riderAudioSource = riderAudioSourceInstance.GetComponent<AudioSource>();

            if (package.callingVehicleClipList == null)
                package.callingVehicleClipList = new List<AudioClip>();

            if (package.callingVehicleClipList.Count == 0)
            {
                package.callingVehicleClipList.Add(
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Whistle/Whistle_01.wav")));
                package.callingVehicleClipList.Add(
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Whistle/Whistle_02.wav")));
                package.callingVehicleClipList.Add(
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Whistle/Whistle_03.wav")));
                package.callingVehicleClipList.Add(
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Whistle/Whistle_04.wav")));
            }


            // ----------------------------------------------------------------------------------------------------
            // Main Camera
            if (!cameraObj.TryGetComponent(out FlareLayer flareLayer))
                cameraObj.AddComponent<FlareLayer>();

            if (useVolumetricHeadLight)
            {
                if (cameraObj.TryGetComponent(out Camera camera))
                    camera.renderingPath = RenderingPath.DeferredShading;

                if (!cameraObj.TryGetComponent(out VolumetricLightRenderer volumetricLightRenderer))
                {
                    volumetricLightRenderer = cameraObj.AddComponent<VolumetricLightRenderer>();
                    volumetricLightRenderer.Resolution = VolumetricLightRenderer.VolumtericResolution.Half;
                    volumetricLightRenderer.DefaultSpotCookie =
                        AssetDatabase.LoadAssetAtPath<Texture>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/SlightlyMad VolumetricLights/Textures/LightsSpot.png"));
                }
            }


            // ----------------------------------------------------------------------------------------------------
            // Animator
            // ----------------------------------------------------------------------------------------------------
            MotorcycleRiderAnimatorParameters();
            MotorcycleRiderBaseLayer();
            MotorcycleRiderUpperBodyLayer();
            MotorcycleRiderUpperBodyOnlyLayer();
            MotorcycleRiderFullBodyLayer();
            MotorcycleRiderAnimatorTransitions();
            MotorcyclePosition();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void MotorcycleRiderAnimatorParameters()
        {
            if (!animatorController.parameters.HasParameter(PARAM_RIDER_STATE))
                animatorController.AddParameter(PARAM_RIDER_STATE, AnimatorControllerParameterType.Int);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void MotorcycleRiderBaseLayer()
        {
            // ----------------------------------------------------------------------------------------------------
            // Animation Clips
            // ----------------------------------------------------------------------------------------------------
            AnimationClip dummyIdleMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/DummyIdle.anim"));

            AnimationClip getOffLMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/GetOff_L.anim"));
            AnimationClip getOffRMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/GetOff_R.anim"));
            AnimationClip getOnFarLMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/GetOnFar_L.anim"));
            AnimationClip getOnFarRMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/GetOnFar_R.anim"));
            AnimationClip getOnNearLMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/GetOnNear_L.anim"));
            AnimationClip getOnNearRMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/GetOnNear_R.anim"));

            AnimationClip ridingBMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_B.anim"));
            AnimationClip ridingFMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_F.anim"));
            AnimationClip ridingIdleMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_Idle.anim"));
            AnimationClip ridingLMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_L.anim"));
            AnimationClip ridingRMotion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_R.anim"));


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion
            // ----------------------------------------------------------------------------------------------------

            // ----------------------------------------------------------------------------------------------------
            // Base Layer IKPass
            animatorController.SetLayerIKPass(base_Root, true);


            // MIS
            base_Locomotion_MIS = base_LocomotionSM.CreateStateMachineIfNotExist(STATE_MIS);


            // ----------------------------------------------------------------------------------------------------
            // Base Layer - MIS-Motorcycle
            base_MotorcycleSM = base_Locomotion_MIS.CreateStateMachineIfNotExist(STATE_MOTORCYCLE);


            // Riding
            base_Riding = base_MotorcycleSM.FindState("Riding");

            if (base_Riding == null)
            {
                base_Riding = base_MotorcycleSM.CreateBlendTree("Riding", out BlendTree blendTree);
                blendTree.blendType = BlendTreeType.FreeformCartesian2D;
                blendTree.blendParameter = PARAM_INPUT_HORIZONTAL;
                blendTree.blendParameterY = PARAM_INPUT_VERTICAL;

                blendTree.useAutomaticThresholds = false;
                blendTree.AddChild(ridingFMotion, new Vector2(0f, 0.5f));
                blendTree.AddChild(ridingLMotion, new Vector2(-0.5f, 0.5f));
                blendTree.AddChild(ridingRMotion, new Vector2(0.5f, 0.5f));
                blendTree.AddChild(ridingIdleMotion, new Vector2(0f, 0f));
                blendTree.AddChild(ridingLMotion, new Vector2(-0.5f, 0f));
                blendTree.AddChild(ridingRMotion, new Vector2(0.5f, 0f));
                blendTree.AddChild(ridingBMotion, new Vector2(0f, -0.5f));
                blendTree.AddChild(ridingBMotion, new Vector2(-0.5f, -0.5f));
                blendTree.AddChild(ridingBMotion, new Vector2(0.5f, -0.5f));

                base_Riding.motion = blendTree;
                base_Riding.iKOnFeet = true;
            }

            // vAnimatorTag
            if (!base_Riding.TryGetStateMachineBehaviour(out vAnimatorTag base_RidingAnimatorTag))
                base_RidingAnimatorTag = base_Riding.AddStateMachineBehaviour<vAnimatorTag>();

            base_RidingAnimatorTag.tags = base_RidingAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            base_RidingAnimatorTag.tags = base_RidingAnimatorTag.tags.AddStringIfNotExist(TAG_LOCK_MOVEMENT);
            base_RidingAnimatorTag.tags = base_RidingAnimatorTag.tags.AddStringIfNotExist(TAG_LOCK_ROTATION);


            // Base - MIS-Motorcycle Dummy Idle
            base_DummyIdle = base_MotorcycleSM.CreateStateIfNotExist("Dummy Idle", dummyIdleMotion);


            // ----------------------------------------------------------------------------------------------------
            // Base - MIS-Motorcycle - GetOn StateMachine
            base_GetOnSM = base_MotorcycleSM.CreateStateMachineIfNotExist("GetOn");

            // vTriggerSoundByState
            if (useMaleVoice)
            {
                base_GetOnSM.AddvTriggerSoundByState(
                    new List<AudioClip>
                    {
                        AssetDatabase.LoadAssetAtPath<AudioClip>(MISFeature.MIS_MOTORCYCLE_PATH + "Runtime/FX/Audio/GetOn/GetOn_Male_01.wav"),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(MISFeature.MIS_MOTORCYCLE_PATH + "Runtime/FX/Audio/GetOn/GetOn_Male_04.wav"),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(MISFeature.MIS_MOTORCYCLE_PATH + "Runtime/FX/Audio/GetOn/GetOn_Male_06.wav")
                    });
            }
            else
            {
                base_GetOnSM.AddvTriggerSoundByState(
                    new List<AudioClip>
                    {
                        AssetDatabase.LoadAssetAtPath<AudioClip>(MISFeature.MIS_MOTORCYCLE_PATH + "Runtime/FX/Audio/GetOn/GetOn_Female_01.wav"),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(MISFeature.MIS_MOTORCYCLE_PATH + "Runtime/FX/Audio/GetOn/GetOn_Female_04.wav"),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(MISFeature.MIS_MOTORCYCLE_PATH + "Runtime/FX/Audio/GetOn/GetOn_Female_06.wav")
                    });
            }


            // GetOnMotorcycle_R_Far
            base_GetOnMotorcycleRFar = base_GetOnSM.CreateStateIfNotExist("GetOnMotorcycle_R_Far", getOnFarRMotion, true);

            // vAnimatorTag
            if (!base_GetOnMotorcycleRFar.TryGetStateMachineBehaviour(out vAnimatorTag base_GetOnMotorcycleRFarAnimatorTag))
                base_GetOnMotorcycleRFarAnimatorTag = base_GetOnMotorcycleRFar.AddStateMachineBehaviour<vAnimatorTag>();

            base_GetOnMotorcycleRFarAnimatorTag.tags = base_GetOnMotorcycleRFarAnimatorTag.tags.AddStringIfNotExist("GetOn");
            base_GetOnMotorcycleRFarAnimatorTag.tags = base_GetOnMotorcycleRFarAnimatorTag.tags.AddStringIfNotExist(TAG_CUSTOM_ACTION);


            // GetOnMotorcycle_L_Far
            base_GetOnMotorcycleLFar = base_GetOnSM.CreateStateIfNotExist("GetOnMotorcycle_L_Far", getOnFarLMotion, true);

            // vAnimatorTag
            if (!base_GetOnMotorcycleLFar.TryGetStateMachineBehaviour(out vAnimatorTag base_GetOnMotorcycleLFarAnimatorTag))
                base_GetOnMotorcycleLFarAnimatorTag = base_GetOnMotorcycleLFar.AddStateMachineBehaviour<vAnimatorTag>();

            base_GetOnMotorcycleLFarAnimatorTag.tags = base_GetOnMotorcycleLFarAnimatorTag.tags.AddStringIfNotExist("GetOn");
            base_GetOnMotorcycleLFarAnimatorTag.tags = base_GetOnMotorcycleLFarAnimatorTag.tags.AddStringIfNotExist(TAG_CUSTOM_ACTION);


            // GetOnMotorcycle_R_Near
            base_GetOnMotorcycleRNear = base_GetOnSM.CreateStateIfNotExist("GetOnMotorcycle_R_Near", getOnNearRMotion, true);

            // vAnimatorTag
            if (!base_GetOnMotorcycleRNear.TryGetStateMachineBehaviour(out vAnimatorTag base_GetOnMotorcycleRNearAnimatorTag))
                base_GetOnMotorcycleRNearAnimatorTag = base_GetOnMotorcycleRNear.AddStateMachineBehaviour<vAnimatorTag>();

            base_GetOnMotorcycleRNearAnimatorTag.tags = base_GetOnMotorcycleRNearAnimatorTag.tags.AddStringIfNotExist("GetOn");
            base_GetOnMotorcycleRNearAnimatorTag.tags = base_GetOnMotorcycleRNearAnimatorTag.tags.AddStringIfNotExist(TAG_CUSTOM_ACTION);


            // GetOnMotorcycle_L_Near
            base_GetOnMotorcycleLNear = base_GetOnSM.CreateStateIfNotExist("GetOnMotorcycle_L_Near", getOnNearLMotion, true);

            // vAnimatorTag
            if (!base_GetOnMotorcycleLNear.TryGetStateMachineBehaviour(out vAnimatorTag base_GetOnMotorcycleLNearAnimatorTag))
                base_GetOnMotorcycleLNearAnimatorTag = base_GetOnMotorcycleLNear.AddStateMachineBehaviour<vAnimatorTag>();

            base_GetOnMotorcycleLNearAnimatorTag.tags = base_GetOnMotorcycleLNearAnimatorTag.tags.AddStringIfNotExist("GetOn");
            base_GetOnMotorcycleLNearAnimatorTag.tags = base_GetOnMotorcycleLNearAnimatorTag.tags.AddStringIfNotExist(TAG_CUSTOM_ACTION);


            // GetOnMotorcycle_Dummy AnimatorState
            base_GetOnMotorcycleDummy = base_GetOnSM.CreateStateIfNotExist("GetOnMotorcycle_Dummy", null, true);

            // vAnimatorTag
            if (!base_GetOnMotorcycleDummy.TryGetStateMachineBehaviour(out vAnimatorTag base_GetOnMotorcycleDummyAnimatorTag))
                base_GetOnMotorcycleDummyAnimatorTag = base_GetOnMotorcycleDummy.AddStateMachineBehaviour<vAnimatorTag>();

            base_GetOnMotorcycleDummyAnimatorTag.tags = base_GetOnMotorcycleDummyAnimatorTag.tags.AddStringIfNotExist("GetOn");
            base_GetOnMotorcycleDummyAnimatorTag.tags = base_GetOnMotorcycleDummyAnimatorTag.tags.AddStringIfNotExist(TAG_CUSTOM_ACTION);


            // ----------------------------------------------------------------------------------------------------
            // Base Layer - MIS-Motorcycle - GetOff
            base_GetOffSM = base_MotorcycleSM.CreateStateMachineIfNotExist("GetOff");


            // GetOffMotorcycle_R
            base_GetOffMotorcycleR = base_GetOffSM.CreateStateIfNotExist("GetOffMotorcycle_R", getOffRMotion, true);

            // vAnimatorTag
            if (!base_GetOffMotorcycleR.TryGetStateMachineBehaviour(out vAnimatorTag base_GetOffMotorcycleRAnimatorTag))
                base_GetOffMotorcycleRAnimatorTag = base_GetOffMotorcycleR.AddStateMachineBehaviour<vAnimatorTag>();

            base_GetOffMotorcycleRAnimatorTag.tags = base_GetOffMotorcycleRAnimatorTag.tags.AddStringIfNotExist("GetOff");
            base_GetOffMotorcycleRAnimatorTag.tags = base_GetOffMotorcycleRAnimatorTag.tags.AddStringIfNotExist(TAG_CUSTOM_ACTION);


            // GetOffMotorcycle_L
            base_GetOffMotorcycleL = base_GetOffSM.CreateStateIfNotExist("GetOffMotorcycle_L", getOffLMotion, true);

            // vAnimatorTag
            if (!base_GetOffMotorcycleL.TryGetStateMachineBehaviour(out vAnimatorTag base_GetOffMotorcycleLAnimatorTag))
                base_GetOffMotorcycleLAnimatorTag = base_GetOffMotorcycleL.AddStateMachineBehaviour<vAnimatorTag>();

            base_GetOffMotorcycleLAnimatorTag.tags = base_GetOffMotorcycleLAnimatorTag.tags.AddStringIfNotExist("GetOff");
            base_GetOffMotorcycleLAnimatorTag.tags = base_GetOffMotorcycleLAnimatorTag.tags.AddStringIfNotExist(TAG_CUSTOM_ACTION);


            // GetOffMotorcycle_Dummy
            base_GetOffMotorcycleDummy = base_GetOffSM.CreateStateIfNotExist("GetOffMotorcycle_Dummy", dummyIdleMotion, true);

            // vAnimatorTag
            if (!base_GetOffMotorcycleDummy.TryGetStateMachineBehaviour(out vAnimatorTag base_GetOffMotorcycleDummyAnimatorTag))
                base_GetOffMotorcycleDummyAnimatorTag = base_GetOffMotorcycleDummy.AddStateMachineBehaviour<vAnimatorTag>();

            base_GetOffMotorcycleDummyAnimatorTag.tags = base_GetOffMotorcycleDummyAnimatorTag.tags.AddStringIfNotExist("GetOff");
            base_GetOffMotorcycleDummyAnimatorTag.tags = base_GetOffMotorcycleDummyAnimatorTag.tags.AddStringIfNotExist(TAG_CUSTOM_ACTION);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void MotorcycleRiderUpperBodyLayer()
        {
#if INVECTOR_SHOOTER
            if (templateType != MISEditor.TemplateType.Shooter)
                return;

            SetupUpperBodyLayer();
            upb_MIS = upb_Root.CreateStateMachineIfNotExist(STATE_MIS);


            // ----------------------------------------------------------------------------------------------------
            // UpperBody - MIS-Motorcycle Aiming
            upb_MotorcycleAimingSM = upb_MIS.CreateStateMachineIfNotExist("Motorcycle Aiming");


            // UpperBody - MIS-Motorcycle Aiming - Aim Upperbody Pose
            upb_MotorcycleAimingSM_AimPose = upb_MotorcycleAimingSM.FindState(UPB_AIMING_POSE);

            if (upb_MotorcycleAimingSM_AimPose == null)
            {
                upb_MotorcycleAimingSM_AimPose = upb_MotorcycleAimingSM.CreateBlendTree(UPB_AIMING_POSE, out BlendTree blendTree);
                blendTree.blendType = BlendTreeType.Simple1D;
                blendTree.blendParameter = PARAM_UPPERBODY_ID;
                blendTree.useAutomaticThresholds = false;

                AnimationClip aimingHandgunMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_Aiming@Handgun.anim"));
                AnimationClip aimingRifleMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_Aiming@Rifle.anim"));
                AnimationClip aimingRPGMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_Aiming@RPG.anim"));
                AnimationClip aimingShotgunMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_Aiming@Shotgun.anim"));
                AnimationClip aimingSniperMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_Aiming@Sniper.anim"));

                blendTree.useAutomaticThresholds = false;
                blendTree.AddChild(aimingHandgunMotion, 1f);
                blendTree.AddChild(aimingRifleMotion, 2f);
                blendTree.AddChild(aimingRPGMotion, 3f);
                blendTree.AddChild(aimingShotgunMotion, 4f);
                blendTree.AddChild(aimingSniperMotion, 5f);

                upb_MotorcycleAimingSM_AimPose.motion = blendTree;
                upb_MotorcycleAimingSM_AimPose.iKOnFeet = true;
            }

            // vAnimatorTag
            if (!upb_MotorcycleAimingSM_AimPose.TryGetStateMachineBehaviour(out vAnimatorTag upb_MotorcycleAimingSM_AimPoseAnimatorTag))
                upb_MotorcycleAimingSM_AimPoseAnimatorTag = upb_MotorcycleAimingSM_AimPose.AddStateMachineBehaviour<vAnimatorTag>();

            upb_MotorcycleAimingSM_AimPoseAnimatorTag.tags = upb_MotorcycleAimingSM_AimPoseAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            upb_MotorcycleAimingSM_AimPoseAnimatorTag.tags = upb_MotorcycleAimingSM_AimPoseAnimatorTag.tags.AddStringIfNotExist(TAG_HEADTRACK);
            upb_MotorcycleAimingSM_AimPoseAnimatorTag.tags = upb_MotorcycleAimingSM_AimPoseAnimatorTag.tags.AddStringIfNotExist(TAG_UPPERBODY_POSE);
            upb_MotorcycleAimingSM_AimPoseAnimatorTag.tags = upb_MotorcycleAimingSM_AimPoseAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_IK);


            // UpperBody - MIS-Motorcycle Aiming - Can't Aim
            upb_MotorcycleAimingSM_CantAim = upb_MotorcycleAimingSM.FindState(UPB_AIMING_CANT_AIM);
            if (upb_MotorcycleAimingSM_CantAim == null)
            {
                upb_MotorcycleAimingSM_CantAim = upb_MotorcycleAimingSM.CreateBlendTree(UPB_AIMING_CANT_AIM, out BlendTree blendTree);
                blendTree.blendType = BlendTreeType.Simple1D;
                blendTree.blendParameter = PARAM_UPPERBODY_ID;
                blendTree.useAutomaticThresholds = false;

                AnimationClip aimingCantHandgunMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_CantAim@Handgun.anim"));
                AnimationClip aimingCantRifleMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_CantAim@Rifle.anim"));
                AnimationClip aimingCantRPGMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_CantAim@RPG.anim"));
                AnimationClip aimingCantShotgunMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_CantAim@Shotgun.anim"));
                AnimationClip aimingCantSniperMotion =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/Riding_CantAim@Sniper.anim"));

                blendTree.AddChild(aimingCantHandgunMotion, 1f);
                blendTree.AddChild(aimingCantRifleMotion, 2f);
                blendTree.AddChild(aimingCantRPGMotion, 3f);
                blendTree.AddChild(aimingCantShotgunMotion, 4f);
                blendTree.AddChild(aimingCantSniperMotion, 5f);

                upb_MotorcycleAimingSM_CantAim.motion = blendTree;
                upb_MotorcycleAimingSM_CantAim.iKOnFeet = true;
            }


            // ----------------------------------------------------------------------------------------------------
            // UpperBody - Aiming
            upb_AimingSM = upb_Root.CreateStateMachineIfNotExist(UPB_AIMING);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void MotorcycleRiderUpperBodyOnlyLayer()
        {
#if INVECTOR_MELEE
            if (templateType == MISEditor.TemplateType.Basic)
                return;

            base.SetupUpperBodyOnlyLayer();


            // ----------------------------------------------------------------------------------------------------
            // Animation Clips
            // ----------------------------------------------------------------------------------------------------
            AnimationClip weakAttack_Sword_A_Motion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/WeakAttack_Sword_A_UpperBodyAttacks.anim"));
            AnimationClip weakAttack_Sword_B_Motion =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Animations/WeakAttack_Sword_B_UpperBodyAttacks.anim"));


            // ----------------------------------------------------------------------------------------------------
            // UpperBodyOnly - WeakAttacks
            upbo_WeakAttacksSM = upbo_Root.CreateStateMachineIfNotExist(WEAKATTACKS);


            // ----------------------------------------------------------------------------------------------------
            // UpperBodyOnly - WeakAttacks - SwordAttack
            upbo_WeakAttacksSM_SwordAttackSM = upbo_WeakAttacksSM.CreateStateMachineIfNotExist("SwordAttack");
            upbo_WeakAttacksSM.AddExitTransitionIfNotExist(upbo_WeakAttacksSM_SwordAttackSM, null);


            // UpperBodyOnly - WeakAttacks - SwordAttack - A
            upbo_WeakAttacksSM_SwordAttackSM_A = upbo_WeakAttacksSM_SwordAttackSM.CreateStateIfNotExist("A", weakAttack_Sword_A_Motion, true, 0.7f);

            // vMeleeAttackControl
            if (!upbo_WeakAttacksSM_SwordAttackSM_A.TryGetStateMachineBehaviour(out vMeleeAttackControl upbo_WeakAttacksSM_SwordAttackSM_AMeleeAttackControl))
            {
                upbo_WeakAttacksSM_SwordAttackSM_AMeleeAttackControl = upbo_WeakAttacksSM_SwordAttackSM_A.AddStateMachineBehaviour<vMeleeAttackControl>();
                upbo_WeakAttacksSM_SwordAttackSM_AMeleeAttackControl.startDamage = 0.6f;
                upbo_WeakAttacksSM_SwordAttackSM_AMeleeAttackControl.endDamage = 0.8f;
                upbo_WeakAttacksSM_SwordAttackSM_AMeleeAttackControl.meleeAttackType = vAttackType.MeleeWeapon;
                upbo_WeakAttacksSM_SwordAttackSM_AMeleeAttackControl.resetAttackTrigger = false;
            }

            // vTriggerSoundByState
            upbo_WeakAttacksSM_SwordAttackSM_A.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Melee Combat/Audio/slash_B.wav")),
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Melee Combat/Audio/slash_C.wav")),
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Melee Combat/Audio/slash_E.wav")),
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Melee Combat/Audio/slash_G.wav"))
                });


            // UpperBodyOnly - WeakAttacks - SwordAttack - B
            upbo_WeakAttacksSM_SwordAttackSM_B = upbo_WeakAttacksSM_SwordAttackSM.CreateStateIfNotExist("B", weakAttack_Sword_B_Motion, true, 0.7f);

            // vMeleeAttackControl
            if (!upbo_WeakAttacksSM_SwordAttackSM_B.TryGetStateMachineBehaviour(out vMeleeAttackControl upbo_WeakAttacksSM_SwordAttackSM_BMeleeAttackControl))
            {
                upbo_WeakAttacksSM_SwordAttackSM_BMeleeAttackControl = upbo_WeakAttacksSM_SwordAttackSM_B.AddStateMachineBehaviour<vMeleeAttackControl>();
                upbo_WeakAttacksSM_SwordAttackSM_BMeleeAttackControl.startDamage = 0.3f;
                upbo_WeakAttacksSM_SwordAttackSM_BMeleeAttackControl.endDamage = 0.6f;
                upbo_WeakAttacksSM_SwordAttackSM_BMeleeAttackControl.meleeAttackType = vAttackType.MeleeWeapon;
                upbo_WeakAttacksSM_SwordAttackSM_BMeleeAttackControl.resetAttackTrigger = true;
            }

            // vTriggerSoundByState
            upbo_WeakAttacksSM_SwordAttackSM_B.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Melee Combat/Audio/slash_B.wav")),
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Melee Combat/Audio/slash_C.wav")),
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Melee Combat/Audio/slash_E.wav")),
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Melee Combat/Audio/slash_G.wav"))
                });
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void MotorcycleRiderFullBodyLayer()
        {
            if (templateType == MISEditor.TemplateType.Basic)
                return;

            base.SetupFullBodyLayer();


            // ----------------------------------------------------------------------------------------------------
            // FullBody - Attacks - WeakAttacks
            fb_WeakAttacksSM = fb_AttacksSM.FindStateMachine("Weak Attacks");
            if (fb_WeakAttacksSM == null)
            {
                fb_WeakAttacksSM = fb_AttacksSM.FindStateMachine(WEAKATTACKS);

                if (fb_WeakAttacksSM == null)
                    fb_WeakAttacksSM = fb_AttacksSM.AddStateMachine(WEAKATTACKS);
            }


            // ----------------------------------------------------------------------------------------------------
            // FullBody - Attacks - StrongAttacks
            fb_StrongAttacksSM = fb_AttacksSM.CreateStateMachineIfNotExist(STRONGATTACKS);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void MotorcycleRiderAnimatorTransitions()
        {
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
            base_Locomotion_MIS.AddExitTransitionIfNotExist(base_MotorcycleSM, null);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion - MIS - Motorcycle
            // ----------------------------------------------------------------------------------------------------

            // GetOnMotorcycle_R_Far To Riding
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.Riding));
            base_GetOnMotorcycleRFar.AddTransitionIfNotExist(base_Riding, conditionList);


            // GetOnMotorcycle_R_Far To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
            base_GetOnMotorcycleRFar.AddExitTransitionIfNotExist(conditionList);


            // GetOnMotorcycle_L_Far To Riding
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.Riding));
            base_GetOnMotorcycleLFar.AddTransitionIfNotExist(base_Riding, conditionList);


            // GetOnMotorcycle_L_Far To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
            base_GetOnMotorcycleLFar.AddExitTransitionIfNotExist(conditionList);


            // GetOnMotorcycleRNearState To Riding
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.Riding));
            base_GetOnMotorcycleRNear.AddTransitionIfNotExist(base_Riding, conditionList);


            // GetOnMotorcycleRNearState To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
            base_GetOnMotorcycleRNear.AddExitTransitionIfNotExist(conditionList);


            // GetOnMotorcycleLNearState To Riding
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.Riding));
            base_GetOnMotorcycleLNear.AddTransitionIfNotExist(base_Riding, conditionList);


            // GetOnMotorcycleLNearState To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
            base_GetOnMotorcycleLNear.AddExitTransitionIfNotExist(conditionList);


            // GetOnMotorcycleDummyState To Riding
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.Riding));
            base_GetOnMotorcycleDummy.AddTransitionIfNotExist(base_Riding, conditionList);


            // Riding To Dummy Idle
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
            base_Riding.AddTransitionIfNotExist(base_DummyIdle, conditionList);


            // Dummy Idle To Exit
            base_DummyIdle.AddExitTransitionIfNotExist(null, true, 0.8f);


            // Riding To GetOff
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.Exit));
            base_Riding.AddTransitionIfNotExist(base_GetOffSM, conditionList);


            // GetOff To Exit
            base_MotorcycleSM.AddExitTransitionIfNotExist(base_GetOffSM, null);


            // GetOffMotorcycleRState To Exit
            base_GetOffMotorcycleR.AddExitTransitionIfNotExist(null, true, 0.85f);


            // GetOffMotorcycleLState To Exit
            base_GetOffMotorcycleL.AddExitTransitionIfNotExist(null, true, 0.85f);


            // GetOffMotorcycleDummyState To Exit
            base_GetOffMotorcycleDummy.AddExitTransitionIfNotExist(null, true, 0.75f);


#if INVECTOR_SHOOTER
            if (templateType == MISEditor.TemplateType.Shooter)
            {
                // ----------------------------------------------------------------------------------------------------
                // UpperBody - MIS-Motorcycle Aiming
                // ----------------------------------------------------------------------------------------------------
                upb_MIS.AddExitTransitionIfNotExist(upb_MotorcycleAimingSM, null);


                // null To Aiming
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_IS_AIMING, AnimatorConditionMode.If, 0f));    // true
                AnimatorStateTransition upbNull2upbAimingSM = upb_NullSM_Null.transitions.FindTransitionIfContains(upb_AimingSM, conditionList);
                if (upbNull2upbAimingSM != null)
                {
                    if (!upbNull2upbAimingSM.HasCondition(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None)))
                        upbNull2upbAimingSM.AddCondition(AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None, PARAM_RIDER_STATE);
                }


                // null To MIS-Motorcycle Aiming
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_IS_AIMING, AnimatorConditionMode.If, 0f));    // true
                conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.NotEqual, (int)MotorcycleRidingState.None));
                upb_NullSM_Null.AddTransitionIfNotExist(upb_MotorcycleAimingSM, conditionList, false, 0.75f, true, 0.1f);


                // MIS-Motorcycle Aiming - Aim Upperbody Pose State To Exit
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_IS_AIMING, AnimatorConditionMode.IfNot, 0f));    // false
                upb_MotorcycleAimingSM_AimPose.AddExitTransitionIfNotExist(conditionList);

                conditionList.Clear();
                conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
                upb_MotorcycleAimingSM_AimPose.AddExitTransitionIfNotExist(conditionList);


                // MIS-Motorcycle Aiming - Can't Aim State To Exit
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_IS_AIMING, AnimatorConditionMode.IfNot, 0f));    // false
                upb_MotorcycleAimingSM_CantAim.AddExitTransitionIfNotExist(conditionList);

                conditionList.Clear();
                conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
                upb_MotorcycleAimingSM_CantAim.AddExitTransitionIfNotExist(conditionList);


                // MIS-Motorcycle Aiming - Aim Upperbody Pose State To Can't Aim State
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_IS_AIMING, AnimatorConditionMode.If, 0f));   // true
                conditionList.Add(Condition(PARAM_CANAIM, AnimatorConditionMode.IfNot, 0f));   // false
                upb_MotorcycleAimingSM_AimPose.AddTransitionIfNotExist(upb_MotorcycleAimingSM_CantAim, conditionList);


                // MIS-Motorcycle Aiming - Can't Aim State To Aim Upperbody Pose State
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_IS_AIMING, AnimatorConditionMode.If, 0f));   // true
                conditionList.Add(Condition(PARAM_CANAIM, AnimatorConditionMode.If, 0f));   // true
                upb_MotorcycleAimingSM_CantAim.AddTransitionIfNotExist(upb_MotorcycleAimingSM_AimPose, conditionList);
            }
#endif

#if INVECTOR_MELEE
            if (templateType == MISEditor.TemplateType.Melee || templateType == MISEditor.TemplateType.Shooter)
            {
                // ----------------------------------------------------------------------------------------------------
                // UpperBodyOnly Layer
                // ----------------------------------------------------------------------------------------------------

                // null To WeakAttacks
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_WEAK_ATTACK, AnimatorConditionMode.If, 0f));   // Trigger
                conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.NotEqual, (int)MotorcycleRidingState.None));
                upbo_NullSM_Null.AddTransitionIfNotExist(upbo_WeakAttacksSM, conditionList);


                // WeakAttacks Entry To SwordAttack
                AnimatorTransition upboWeakAttacks2upboSwordAttack = upbo_WeakAttacksSM.AddEntryTransition(upbo_WeakAttacksSM_SwordAttackSM_A);
                upboWeakAttacks2upboSwordAttack.AddCondition(AnimatorConditionMode.Equals, 1f, PARAM_ATTACK_ID);


                // WeakAttack - SwordAttack - A To B
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_WEAK_ATTACK, AnimatorConditionMode.If, 0f));   // Trigger
                upbo_WeakAttacksSM_SwordAttackSM_A.AddTransitionIfNotExist(upbo_WeakAttacksSM_SwordAttackSM_B, conditionList, true, 0.8f);

                // WeakAttack - SwordAttack - A To Exit
                upbo_WeakAttacksSM_SwordAttackSM_A.AddExitTransitionIfNotExist(null, true, 0.9f);


                // WeakAttack - SwordAttack - B To Exit
                upbo_WeakAttacksSM_SwordAttackSM_B.AddExitTransitionIfNotExist(null, true, 0.7f);


                // ----------------------------------------------------------------------------------------------------
                // FullBody Layer
                // ----------------------------------------------------------------------------------------------------

                // null To WeakAttacks
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_WEAK_ATTACK, AnimatorConditionMode.If, 0f));   // Trigger
                conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
                AnimatorStateTransition fbNull2fbWeakAttacks = fb_NullSM_NULL.transitions.FindSameTransition(fb_WeakAttacksSM, conditionList);

                if (fbNull2fbWeakAttacks == null)
                {
                    conditionList.Clear();
                    conditionList.Add(Condition(PARAM_WEAK_ATTACK, AnimatorConditionMode.If, 0f));   // Trigger
                    fbNull2fbWeakAttacks = fb_NullSM_NULL.transitions.FindTransitionIfContains(fb_WeakAttacksSM, conditionList);

                    if (fbNull2fbWeakAttacks == null)
                    {
                        conditionList.Clear();
                        conditionList.Add(Condition(PARAM_WEAK_ATTACK, AnimatorConditionMode.If, 0f));   // Trigger
                        conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
                        fb_NullSM_NULL.AddTransition(fb_WeakAttacksSM, conditionList);
                    }
                    else
                    {
                        fbNull2fbWeakAttacks.AddCondition(AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None, PARAM_RIDER_STATE);
                    }
                }

                // null To StrongAttacks
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_STRONG_ATTACK, AnimatorConditionMode.If, 0f));   // Trigger
                conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
                AnimatorStateTransition fbNull2fbStrongAttacks = fb_NullSM_NULL.transitions.FindSameTransition(fb_StrongAttacksSM, conditionList);

                if (fbNull2fbStrongAttacks == null)
                {
                    conditionList.Clear();
                    conditionList.Add(Condition(PARAM_STRONG_ATTACK, AnimatorConditionMode.If, 0f));   // Trigger
                    fbNull2fbStrongAttacks = fb_NullSM_NULL.transitions.FindTransitionIfContains(fb_StrongAttacksSM, conditionList);

                    if (fbNull2fbStrongAttacks == null)
                    {
                        conditionList.Clear();
                        conditionList.Add(Condition(PARAM_STRONG_ATTACK, AnimatorConditionMode.If, 0f));   // Trigger
                        conditionList.Add(Condition(PARAM_RIDER_STATE, AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None));
                        fb_NullSM_NULL.AddTransition(fb_StrongAttacksSM, conditionList);
                    }
                    else
                    {
                        fbNull2fbStrongAttacks.AddCondition(AnimatorConditionMode.Equals, (int)MotorcycleRidingState.None, PARAM_RIDER_STATE);
                    }
                }
            }
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void MotorcyclePosition()
        {
            // ----------------------------------------------------------------------------------------------------
            // Base Layer
            // ----------------------------------------------------------------------------------------------------


            // ----------------------------------------------------------------------------------------------------
            // Locomotion
            base_LocomotionSM.SetStateMachinePosition(base_Locomotion_MIS, BASE_LOCOMOTION_MIS_POS);
            base_Locomotion_MIS.SetDefaultLayerAllPosition();
            base_Locomotion_MIS.ArrangeStatemachines(0);


            // ----------------------------------------------------------------------------------------------------
            // Base - MIS-Motorcycle
            // ----------------------------------------------------------------------------------------------------
            base_MotorcycleSM.SetDefaultLayerPosition(BASE_LD_POS, BASE_RM_POS);
            base_MotorcycleSM.SetParentStateMachinePosition(ZERO_POS);

            base_MotorcycleSM.SetStateRelativePosition(base_Riding, 0, 0);
            base_MotorcycleSM.SetStateRelativePosition(base_DummyIdle, 3, -1);


            // ----------------------------------------------------------------------------------------------------
            // Base - MIS-Motorcycle - GetOn
            base_MotorcycleSM.SetStateMachineRelativePosition(base_GetOnSM, 0, -1);

            base_GetOnSM.SetDefaultLayerPosition(BASE_LD_POS, BASE_RM_POS);
            base_GetOnSM.SetParentStateMachinePosition(ZERO_POS);

            base_GetOnSM.SetStateRelativePosition(base_GetOnMotorcycleRFar, 0, -2);
            base_GetOnSM.SetStateRelativePosition(base_GetOnMotorcycleLFar, 0, -1);
            base_GetOnSM.SetStateRelativePosition(base_GetOnMotorcycleDummy, 0, 0);
            base_GetOnSM.SetStateRelativePosition(base_GetOnMotorcycleRNear, 0, 1);
            base_GetOnSM.SetStateRelativePosition(base_GetOnMotorcycleLNear, 0, 2);


            // ----------------------------------------------------------------------------------------------------
            // Base - MIS-Motorcycle - GetOff
            base_MotorcycleSM.SetStateMachineRelativePosition(base_GetOffSM, 0, 1);

            base_GetOffSM.SetDefaultLayerPosition(BASE_LD_POS, BASE_RM_POS);
            base_GetOffSM.SetParentStateMachinePosition(ZERO_POS);

            base_GetOffSM.SetStateRelativePosition(base_GetOffMotorcycleR, 0, -1);
            base_GetOffSM.SetStateRelativePosition(base_GetOffMotorcycleDummy, 0, 0);
            base_GetOffSM.SetStateRelativePosition(base_GetOffMotorcycleL, 0, 1);

#if INVECTOR_SHOOTER
            // ----------------------------------------------------------------------------------------------------
            // UpperBody
            // ----------------------------------------------------------------------------------------------------
            if (templateType == MISEditor.TemplateType.Shooter)
            {
                // ----------------------------------------------------------------------------------------------------
                // MIS
                upb_Root.SetStateMachinePosition(upb_MIS, UPB_MIS_POS);
                upb_MIS.SetDefaultLayerAllPosition();
                upb_MIS.ArrangeStatemachines(0);


                // ----------------------------------------------------------------------------------------------------
                // MIS - Motorcycle Aiming
                upb_MotorcycleAimingSM.SetDefaultLayerAllPosition();

                upb_MotorcycleAimingSM.SetStateRelativePosition(upb_MotorcycleAimingSM_AimPose, 0, 0);
                upb_MotorcycleAimingSM.SetStateRelativePosition(upb_MotorcycleAimingSM_CantAim, 0, 1);
            }
#endif

#if INVECTOR_MELEE || INVECTOR_SHOOTER
            if (templateType == MISEditor.TemplateType.Melee || templateType == MISEditor.TemplateType.Shooter)
            {
                // ----------------------------------------------------------------------------------------------------
                // UpperBodyOnly
                // ----------------------------------------------------------------------------------------------------
                upbo_Root.SetStateMachinePosition(upbo_WeakAttacksSM, UPBO_WEAK_ATTACKS_POS);

                upbo_WeakAttacksSM.SetDefaultLayerPosition(BASE_LU_POS, BASE_RM_POS);
                upbo_WeakAttacksSM.SetParentStateMachinePosition(ZERO_POS);
                upbo_WeakAttacksSM.SetStateMachinePosition(upbo_WeakAttacksSM_SwordAttackSM, STATE_POS);

                upbo_WeakAttacksSM_SwordAttackSM.SetDefaultLayerPosition(BASE_LU_POS, BASE_RM_POS);
                upbo_WeakAttacksSM_SwordAttackSM.SetParentStateMachinePosition(ZERO_POS);

                upbo_WeakAttacksSM_SwordAttackSM.SetStateRelativePosition(upbo_WeakAttacksSM_SwordAttackSM_A, 0, 0);
                upbo_WeakAttacksSM_SwordAttackSM.SetStateRelativePosition(upbo_WeakAttacksSM_SwordAttackSM_B, 0, -1);
            }
#endif
        }
#endif
    }
}
