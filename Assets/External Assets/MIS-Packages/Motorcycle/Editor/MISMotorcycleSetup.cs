#pragma warning disable 0618

using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public class MISMotorcycleSetup : MISSetupBase
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // Motorcycle Objects
        public float motorcycleMass = 800f;
        public GameObject bodyObj;
        public GameObject steerObj;
        public GameObject spineObj;
        public GameObject axleFrontObj;
        public GameObject wheelFrontObj;
        public GameObject axleRearObj;
        public GameObject shockAbsorberRearObj;
        public GameObject wheelRearObj;

        // ----------------------------------------------------------------------------------------------------
        // FX
        bool useHeadLight = true;
        bool useBrakeLight = true;

        bool useGroundSmoke = true;
        bool useBoostSpark = true;
        bool useBoostExplosion = true;
        bool useExhaust = true;
        bool useJumpForce = true;

        bool useSpeedometer = true;
        bool useVehicleGlass = true;

        // ----------------------------------------------------------------------------------------------------
        // Textures
        public static Texture harleyFullGray;
        public static Texture body, bodyEmpty;
        public static Texture steer, steerEmpty;
        public static Texture spine, spineEmpty;
        public static Texture suspensionFront, suspensionFrontEmpty;
        public static Texture wheelFront, wheelFrontEmpty;
        public static Texture axleRear, axleRearEmpty;
        public static Texture suspensionRear, suspensionRearEmpty;
        public static Texture wheelRear, wheelRearEmpty;
        Rect harleyRect = new Rect(0, 0, 512, 256);

        // ----------------------------------------------------------------------------------------------------
        // 
        GameObject motorcycleObj;
        GameObject bodyParentObj;
        GameObject axleRearPivotObj;
        GameObject sockabsorberRearPivotObj;
        GameObject suspensionRearPivotObj;
        bool allPartsReady = false;

        GameObject wheelCollidersObj;
        GameObject comObj;
        GameObject seatObj;
        GameObject handIKTargetsObj, footIKTargetsObj, spineIKTargetObj;
        GameObject entryPointsObj;

        GameObject headLightObj;
        GameObject brakeLightObj;

        GameObject GroundSmokeFObj, GroundSmokeRObj;
        GameObject BoostSparkObj;
        GameObject BoostExplosionObj;
        GameObject ExhaustObj;
        GameObject JumpForceObj;

        GameObject speedometerObj;
        GameObject vehicleGlassObj;

        AudioSource idleSource, lowSource, midSource, highSource, reverseSource, nonSpatialSource;

        // ----------------------------------------------------------------------------------------------------
        // 
        mvMotorcycleWheel motorcycleWheelFront, motorcycleWheelRear;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        [MenuItem("Tools/MIS/Motorcycle Setup", false, (int)MISEditor.MISMenuItem.SubPackageSetup)]
        public static void ShowWindow()
        {
            GetWindow(typeof(MISMotorcycleSetup), false, "Motorcycle Setup");
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void OnEnable()
        {
            base.OnEnable();

            minWidth = 512;
            minHeight = 720;
            minSize = new Vector2(minWidth, minHeight);

            misBanner = (Texture2D)Resources.Load("MIS_SetupBanner_Motorcycle", typeof(Texture2D));

            //SetTitleVersion(MISFeature.MIS_PACKAGE_MOTORCYCLE, MISMotorcycle.PACKAGE_VERSION);

            harleyFullGray = (Texture)Resources.Load("Harley_FullGray", typeof(Texture));

            body = (Texture)Resources.Load("Body", typeof(Texture));
            bodyEmpty = (Texture)Resources.Load("Body_Empty", typeof(Texture));

            steer = (Texture)Resources.Load("Steer", typeof(Texture));
            steerEmpty = (Texture)Resources.Load("Steer_Empty", typeof(Texture));

            suspensionFront = (Texture)Resources.Load("Suspension_F", typeof(Texture));
            suspensionFrontEmpty = (Texture)Resources.Load("Suspension_F_Empty", typeof(Texture));

            wheelFront = (Texture)Resources.Load("Wheel_F", typeof(Texture));
            wheelFrontEmpty = (Texture)Resources.Load("Wheel_F_Empty", typeof(Texture));

            axleRear = (Texture)Resources.Load("Axle_R", typeof(Texture));
            axleRearEmpty = (Texture)Resources.Load("Axle_R_Empty", typeof(Texture));

            suspensionRear = (Texture)Resources.Load("ShockAbsorber_R", typeof(Texture));
            suspensionRearEmpty = (Texture)Resources.Load("ShockAbsorber_R_Empty", typeof(Texture));

            wheelRear = (Texture)Resources.Load("Wheel_R", typeof(Texture));
            wheelRearEmpty = (Texture)Resources.Load("Wheel_R_Empty", typeof(Texture));
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Label(misBanner, /*GUILayout.ExpandWidth(true), */GUILayout.Height(80));

            GUILayout.Space(-15);

            GUILayout.BeginVertical(MISFeature.MIS_PACKAGE_MOTORCYCLE, skin.GetStyle("WindowBG"), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                {
                    // Objects
                    motorcycleMass = EditorGUILayout.FloatField(new GUIContent("Motorcycle Mass"), motorcycleMass);
                    bodyObj = EditorGUILayout.ObjectField("Body", bodyObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                    steerObj = EditorGUILayout.ObjectField("Steer", steerObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                    spineObj = EditorGUILayout.ObjectField("Spine", spineObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                    axleFrontObj = EditorGUILayout.ObjectField("Front Axle", axleFrontObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                    wheelFrontObj = EditorGUILayout.ObjectField("Front Wheel", wheelFrontObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                    axleRearObj = EditorGUILayout.ObjectField("Rear Axle", axleRearObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                    shockAbsorberRearObj = EditorGUILayout.ObjectField("Rear ShockAbsorber", shockAbsorberRearObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                    wheelRearObj = EditorGUILayout.ObjectField("Rear Wheel", wheelRearObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

                    // Verify
                    VerifyParts();

                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    {
                        MISEditor.SetToggleLabelWidth(true);
                        {
                            useHeadLight = EditorGUILayout.Toggle("Use Head Light", useHeadLight);
                            useBrakeLight = EditorGUILayout.Toggle("Use Brake Light", useBrakeLight);

                            useGroundSmoke = EditorGUILayout.Toggle("Use Ground Smoke", useGroundSmoke);
                            useBoostSpark = EditorGUILayout.Toggle("Use Boost Spark", useBoostSpark);
                            useBoostExplosion = EditorGUILayout.Toggle("Use Boost Explosion", useBoostExplosion);
                            useExhaust = EditorGUILayout.Toggle("Use Exhaust", useExhaust);
                            useJumpForce = EditorGUILayout.Toggle("Use Jump Force", useJumpForce);

                            useSpeedometer = EditorGUILayout.Toggle("Use Speedometer UI", useSpeedometer);
                            useVehicleGlass = EditorGUILayout.Toggle("Use Vehicle Glass UI", useVehicleGlass);
                        }
                        MISEditor.SetToggleLabelWidth(false);

                        GUILayout.Space(10);

                        EditorGUI.BeginDisabledGroup(!allPartsReady);
                        {
                            GUILayout.Space(10);

                            if (GUILayout.Button("Setup"))
                            {
#if UNITY_2018_3_OR_NEWER
                                PrefabAssetType m_AssetType = PrefabUtility.GetPrefabAssetType(bodyObj);

                                if (m_AssetType != PrefabAssetType.NotAPrefab)
                                {
                                    GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(bodyObj);
                                    PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                                }
#endif

                                MotorcycleSetup();
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void VerifyParts()
        {
            if (bodyObj == null || steerObj == null || spineObj == null || wheelFrontObj == null || wheelRearObj == null)
                allPartsReady = false;
            else
                allPartsReady = true;

            harleyRect = new Rect((EditorGUIUtility.currentViewWidth - (EditorGUIUtility.currentViewWidth * 0.8f)) * 0.5f, 260, EditorGUIUtility.currentViewWidth * 0.8f, EditorGUIUtility.currentViewWidth * 0.8f * 0.5f);
            GUI.DrawTexture(harleyRect, harleyFullGray);

            if (bodyObj)
                GUI.DrawTexture(harleyRect, body);
            else
                GUI.DrawTexture(harleyRect, bodyEmpty);

            if (steerObj)
                GUI.DrawTexture(harleyRect, steer);
            else
                GUI.DrawTexture(harleyRect, steerEmpty);
            if (spineObj)
                GUI.DrawTexture(harleyRect, spine);
            else
                GUI.DrawTexture(harleyRect, spineEmpty);
            if (wheelFrontObj)
                GUI.DrawTexture(harleyRect, wheelFront);
            else
                GUI.DrawTexture(harleyRect, wheelFrontEmpty);

            if (axleFrontObj)
                GUI.DrawTexture(harleyRect, suspensionFront);
            else
                GUI.DrawTexture(harleyRect, suspensionFrontEmpty);

            if (wheelRearObj)
                GUI.DrawTexture(harleyRect, wheelRear);
            else
                GUI.DrawTexture(harleyRect, wheelRearEmpty);

            if (axleRearObj)
                GUI.DrawTexture(harleyRect, axleRear);
            else
                GUI.DrawTexture(harleyRect, axleRearEmpty);

            if (shockAbsorberRearObj)
                GUI.DrawTexture(harleyRect, suspensionRear);
            else
                GUI.DrawTexture(harleyRect, suspensionRearEmpty);
        }

        // ----------------------------------------------------------------------------------------------------
        // - Vehicle 레이어 추가해야 한다.
        // - 셋업 윈도우 백그라운드에 셋업이 필요한 오토바이 부속 그림을 배경으로 깔고 해당 부품 오브젝트를 드래그&드롭하면
        //   머티리얼을 렌더링해서 보여주면 좋겠다.
        //   바디, 앞바퀴/뒷바퀴, 스티어링휠
        // ----------------------------------------------------------------------------------------------------
        void MotorcycleSetup()
        {
            SetupStructure();
            SetupObject();
            SetupScript();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            MISSetupCompletePopup.Open();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void SetupStructure()
        {
            bodyObj.transform.parent = null;

            motorcycleObj = new GameObject("New MIS-Motorcycle");
            motorcycleObj.transform.position = bodyObj.transform.position;
            motorcycleObj.transform.rotation = bodyObj.transform.rotation;

            bodyParentObj = new GameObject("Model");
            bodyParentObj.transform.SetLocalParent(motorcycleObj.transform);

            bodyObj.transform.SetParent(bodyParentObj.transform);


            // ----------------------------------------------------------------------------------------------------
            // Layer Setting
            Transform[] children = motorcycleObj.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
                child.gameObject.layer = LayerMask.NameToLayer(MISEditorTagLayer.LAYER_VEHICLE);


            steerObj.transform.SetParent(bodyObj.transform);

            if (axleFrontObj)
            {
                axleFrontObj.transform.SetParent(steerObj.transform);
                wheelFrontObj.transform.SetParent(axleFrontObj.transform);
            }
            else
            {
                wheelFrontObj.transform.SetParent(steerObj.transform);
            }

            if (axleRearObj)
            {
                axleRearPivotObj = new GameObject("PivotAxle_R");
                axleRearPivotObj.transform.SetParent(bodyObj.transform);
                axleRearPivotObj.transform.localPosition = axleRearObj.transform.localPosition;
                axleRearPivotObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                axleRearObj.transform.SetParent(axleRearPivotObj.transform);
            }

            suspensionRearPivotObj = new GameObject("Suspension_R");
            suspensionRearPivotObj.transform.SetParent(bodyObj.transform);
            suspensionRearPivotObj.transform.localPosition = wheelRearObj.transform.localPosition;

            if (shockAbsorberRearObj)
            {
                suspensionRearPivotObj.transform.localRotation = Quaternion.LookRotation(shockAbsorberRearObj.transform.forward);

                sockabsorberRearPivotObj = new GameObject("PivotShockAbsorber_R");
                sockabsorberRearPivotObj.transform.SetParent(bodyObj.transform);
                sockabsorberRearPivotObj.transform.localPosition = shockAbsorberRearObj.transform.localPosition;
                sockabsorberRearPivotObj.transform.localRotation = Quaternion.LookRotation(-shockAbsorberRearObj.transform.up);
                shockAbsorberRearObj.transform.SetParent(sockabsorberRearPivotObj.transform);
            }
            else
            {
                suspensionRearPivotObj.transform.localRotation = Quaternion.identity;
            }

            wheelRearObj.transform.SetParent(suspensionRearPivotObj.transform);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void SetupObject()
        {
            GameObject prefab;


            // ----------------------------------------------------------------------------------------------------
            // Containers
            misComponentsParentObj = new GameObject(MIS_COMPONENTS);
            misComponentsParentObj.transform.SetParent(motorcycleObj.transform);
            misComponentsParentObj.transform.localPosition = Vector3.zero;
            misComponentsParentObj.transform.localRotation = Quaternion.identity;

            Transform vfxComponents = new GameObject("FX").transform;
            vfxComponents.SetParent(misComponentsParentObj.transform);
            vfxComponents.transform.localPosition = Vector3.zero;
            vfxComponents.transform.localRotation = Quaternion.identity;

            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/UI.prefab"));
            GameObject uiInstance = prefab.Instantiate2D(Vector3.zero, misComponentsParentObj.transform);


            // ----------------------------------------------------------------------------------------------------
            // Object Components

            // WheelColliders
            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/WheelColliders.prefab"));
            wheelCollidersObj = prefab.Instantiate3D(Vector3.zero, motorcycleObj.transform);

            // CenterOfMass
            comObj = new GameObject("CenterOfMass");
            comObj.transform.SetParent(motorcycleObj.transform);
            comObj.transform.localPosition = new Vector3(0f, 0f, 0.89f);
            comObj.transform.localRotation = Quaternion.identity;

            // PivotSeat
            seatObj = new GameObject("PivotSeat");
            seatObj.transform.SetParent(bodyObj.transform);
            seatObj.transform.localPosition = new Vector3(0f, 0.925f, 0.07f);
            seatObj.transform.localRotation = Quaternion.identity;

            // Hand IK Targets
            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/HandIKTargets.prefab"));
            handIKTargetsObj = prefab.Instantiate3D(Vector3.zero, steerObj.transform);

            // Foot IK Targets
            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/FootIKTargets.prefab"));
            footIKTargetsObj = prefab.Instantiate3D(Vector3.zero, bodyObj.transform);

            // Spine IK Targets
            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/SpineIKTarget.prefab"));
            spineIKTargetObj = prefab.Instantiate3D(Vector3.zero, spineObj.transform);

            // EntryPoints
            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/EntryPoints.prefab"));
            entryPointsObj = prefab.Instantiate3D(Vector3.zero, misComponentsParentObj.transform);


            // ----------------------------------------------------------------------------------------------------
            // FX Components

            // Head Lights
            if (useHeadLight)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Lights/HeadLight.prefab"));
                headLightObj = prefab.Instantiate3D(Vector3.zero, steerObj.transform);
                headLightObj.transform.localPosition = new Vector3(0f, -0.088f, 0.196f);
                headLightObj.transform.localEulerAngles = new Vector3(60f, 0f, 0f);
            }

            // Brake Light
            if (useBrakeLight)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Lights/BrakeLight.prefab"));
                brakeLightObj = prefab.Instantiate3D(Vector3.zero, bodyObj.transform);
                brakeLightObj.transform.localPosition = new Vector3(0f, 0.69f, -0.8f);
                brakeLightObj.transform.localRotation = Quaternion.identity;
            }

            // GroundSmoke
            if (useGroundSmoke)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/GroundSmoke/GroundSmoke.prefab"));
                GroundSmokeFObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
                GroundSmokeFObj.transform.localPosition = new Vector3(0f, 0f, 1.9f);
                GroundSmokeFObj.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/GroundSmoke/GroundSmoke.prefab"));
                GroundSmokeRObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
                GroundSmokeRObj.transform.localPosition = new Vector3(0f, 0f, -0.54f);
                GroundSmokeRObj.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            }

            // BoostSpark
            if (useBoostSpark)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Boost Spark/BoostSpark.prefab"));
                BoostSparkObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
                BoostSparkObj.transform.localPosition = new Vector3(0f, 0.1f, 0.6f);
                BoostSparkObj.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            }

            // BoostExplosion
            if (useBoostExplosion)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Boost Explosion/BoostExplosions.prefab"));
                BoostExplosionObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
            }

            // Exhaust
            if (useExhaust)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Exhaust/ExhaustManager.prefab"));
                ExhaustObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
            }

            // Jump Force
            if (useJumpForce)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/JumpForce/JumpForce.prefab"));
                JumpForceObj = prefab.Instantiate3D(Vector3.zero, wheelRearObj.transform);
            }


            // ----------------------------------------------------------------------------------------------------
            // Audio Sources
            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/EngineIdleSource.prefab"));
            GameObject idleSourceObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
            idleSource = idleSourceObj.GetComponent<AudioSource>();
            idleSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_EngineIdle.wav"));

            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/EngineLowSource.prefab"));
            GameObject lowSourceObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
            lowSource = lowSourceObj.GetComponent<AudioSource>();
            lowSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_EngineLow.wav"));

            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/EngineMidSource.prefab"));
            GameObject midSourceObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
            midSource = midSourceObj.GetComponent<AudioSource>();
            midSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_EngineMid.wav"));

            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/EngineHighSource.prefab"));
            GameObject highSourceObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
            highSource = highSourceObj.GetComponent<AudioSource>();
            highSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_EngineHigh.wav"));

            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/EngineReverseSource.prefab"));
            GameObject reverseSourceObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
            reverseSource = reverseSourceObj.GetComponent<AudioSource>();
            reverseSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_EngineIdle.wav"));

            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Prefabs/NonSpatialSource.prefab"));
            GameObject nonSpatialSourceObj = prefab.Instantiate3D(Vector3.zero, vfxComponents);
            nonSpatialSource = nonSpatialSourceObj.GetComponent<AudioSource>();


            // ----------------------------------------------------------------------------------------------------
            // UI Components

            if (useSpeedometer)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/Scripts/UI/Speedometer/Speedometer.prefab"));
                speedometerObj = prefab.Instantiate2D(new Vector3(-60f, -60f, 0f), uiInstance.transform);
            }

            if (useVehicleGlass)
            {
                prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/VehicleGlass/VehicleGlass.prefab"));
                vehicleGlassObj = prefab.Instantiate2D(Vector3.zero, uiInstance.transform);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void SetupScript()
        {
            // Layer
            motorcycleObj.layer = LayerMask.NameToLayer(MISEditorTagLayer.LAYER_VEHICLE);


            // ----------------------------------------------------------------------------------------------------
            // Capsule Collider
            CapsuleCollider collider = motorcycleObj.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.5f, 0.4f);
            collider.radius = 0.35f;
            collider.height = 2.5f;
            collider.direction = 2; // Z axis


            // ----------------------------------------------------------------------------------------------------
            // Rigidbody
            Rigidbody _rigidbody = motorcycleObj.AddComponent<Rigidbody>();
            _rigidbody.mass = motorcycleMass;
            _rigidbody.drag = 0f;
            _rigidbody.angularDrag = 0.05f;
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;


            // ----------------------------------------------------------------------------------------------------
            // mvMotorcycleInput
            mvMotorcycleInput vcInput = motorcycleObj.AddComponent<mvMotorcycleInput>();


            // ----------------------------------------------------------------------------------------------------
            // mvMotorcycle
            mvMotorcycle vc = motorcycleObj.AddComponent<mvMotorcycle>();
            vc.bodyTransform = bodyParentObj.transform;
            vc.centerOfMass = comObj.transform;
            vc.steeringWheel = steerObj.transform;

            vc.ikLeftHand = handIKTargetsObj.transform.Find("IKHand_L").transform;
            vc.ikRightHand = handIKTargetsObj.transform.Find("IKHand_R").transform;
            vc.ikLeftFoot = footIKTargetsObj.transform.Find("IKFoot_L").transform;
            vc.ikRightFoot = footIKTargetsObj.transform.Find("IKFoot_R").transform;
            vc.ikSpineHint = spineIKTargetObj.transform.Find("IkSpine").transform;

            vc.wheelList.Clear();
            motorcycleWheelFront = wheelCollidersObj.transform.Find("Collider_F").gameObject.GetComponent<mvMotorcycleWheel>();
            vc.wheelList.Add(motorcycleWheelFront);
            motorcycleWheelRear = wheelCollidersObj.transform.Find("Collider_R").gameObject.GetComponent<mvMotorcycleWheel>();
            vc.wheelList.Add(motorcycleWheelRear);

            vc.vehicleGlassPanel = useVehicleGlass ? vehicleGlassObj : null;
            vc.useSpeedometer = useSpeedometer;

            vc.wheelFrictionData =
                AssetDatabase.LoadAssetAtPath<mvWheelFrictionData>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/WheelFrictionData/WheelFrictionData.asset"));

            vc.groundData =
                AssetDatabase.LoadAssetAtPath<mvGroundData>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/GroundData/GroundData.asset"));


            // ----------------------------------------------------------------------------------------------------
            // mvEngineSound
            mvEngineSound vcES = motorcycleObj.AddComponent<mvEngineSound>();
            AudioMixer audioMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(Path.Combine(MISEditor.MIS_ASSETS_PATH, "MIS/Scripts/FX/Audio/MISAudioMixer.mixer"));
            vcES.audioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];

            vcES.acEngineStart =
                AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_EngineStart.wav"));
            vcES.acEngineStop =
                AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_EngineStop.wav"));
            vcES.acBrake =
                AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_Brake.wav"));
            vcES.acGearShift =
                AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISFeature.MIS_MOTORCYCLE_PATH, "Runtime/FX/Audio/Chopper/Chopper_GearShift.wav"));

            vcES.idleAudioSource = idleSource;
            vcES.lowAudioSource = lowSource;
            vcES.midAudioSource = midSource;
            vcES.highAudioSource = highSource;
            vcES.reverseAudioSource = reverseSource;
            vcES.nonSpatialSource = nonSpatialSource;


            // ----------------------------------------------------------------------------------------------------
            // mvVehicleGetOnOff
            mvVehicleGetOnOff vcGetOnOff = motorcycleObj.AddComponent<mvVehicleGetOnOff>();
            vcGetOnOff.entryPoints = new EntryPoint[2];

            vcGetOnOff.entryPoints[0] = new EntryPoint
            {
                entryState = EntryState.None,
                point = entryPointsObj.transform.Find("Entry_L"),
                side = EntrySide.Left,
                seat = seatObj.transform,
                hasTaken = false,
                useAnimation = true,
                endExitTimeAnimation = 0.9f,
                cameraState = "Motorcycle"
            };

            vcGetOnOff.entryPoints[1] = new EntryPoint
            {
                entryState = EntryState.None,
                point = entryPointsObj.transform.Find("Entry_R"),
                side = EntrySide.Right,
                seat = seatObj.transform,
                hasTaken = false,
                useAnimation = true,
                endExitTimeAnimation = 0.9f,
                cameraState = "Motorcycle"
            };


            // ----------------------------------------------------------------------------------------------------
            // mvJumpForce
            if (useJumpForce)
            {
                mvJumpForce jumpForce = motorcycleObj.AddComponent<mvJumpForce>();
                jumpForce.onGroundVfx = JumpForceObj.transform.Find("Smoke").gameObject;
                jumpForce.onAirVfx = JumpForceObj.transform.Find("Shockwave").gameObject;

                // mvMotorcycle
                UnityAction<bool> startOneTimeForceDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<bool>), jumpForce, "StartOneTimeForce") as UnityAction<bool>;

                vc.OnJumpOnGround.RemoveMissingPersistents();
                UnityEventTools.AddBoolPersistentListener(vc.OnJumpOnGround, startOneTimeForceDelegate, true);

                vc.OnJumpOnAir.RemoveMissingPersistents();
                UnityEventTools.AddBoolPersistentListener(vc.OnJumpOnAir, startOneTimeForceDelegate, false);
            }


            /*
            // ----------------------------------------------------------------------------------------------------
            // mvVehiclePhysicalDamage
            mvVWDealPhysicalDamage vcPhysicalDamage = motorcycleObj.AddComponent<mvVWDealPhysicalDamage>();
            vcPhysicalDamage.damage = new Invector.vDamage
            {
                damageType = null,
                damageValue = 100,
                staminaBlockCost = 5f,
                staminaRecoveryDelay = 1f,
                ignoreDefense = true,
                activeRagdoll = false
            };

            //vcPhysicalDamage.triggerLayerMask = LayerMask.NameToLayer(MISEditorTagLayer.LAYER_ENEMY);
            vcPhysicalDamage.triggerTag = new vTagMask
            {
                MISEditorTagLayer.TAG_BOSS,
                MISEditorTagLayer.TAG_ENEMY
            };

            vcPhysicalDamage.externalForce = 0.5f;
            vcPhysicalDamage.upwardsModifier = 50f;
            vcPhysicalDamage.maxForceMagnitude = 12f;
            vcPhysicalDamage.damageVelocity = 8f;
            vcPhysicalDamage.useRelativeDamage = false;
            vcPhysicalDamage.activateRagdoll = true;*/


            // ----------------------------------------------------------------------------------------------------
            // Wheel MeshCollider
            if (wheelFrontObj)
            {
                MeshCollider meshCollider = wheelFrontObj.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.enabled = false;
            }

            if (wheelRearObj)
            {
                MeshCollider meshCollider = wheelRearObj.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.enabled = false;
            }


            // ----------------------------------------------------------------------------------------------------
            // mvWheelCollider
            mvMotorcycleWheel wheelColliderFront = wheelCollidersObj.transform.Find("Collider_F").gameObject.GetComponent<mvMotorcycleWheel>();
            wheelColliderFront.wheelModel = wheelFrontObj.transform;
            if (axleFrontObj)
                wheelColliderFront.suspension = axleFrontObj.transform;

            mvMotorcycleWheel wheelColliderRear = wheelCollidersObj.transform.Find("Collider_R").gameObject.GetComponent<mvMotorcycleWheel>();
            wheelColliderRear.wheelModel = wheelRearObj.transform;
            wheelColliderRear.suspension = suspensionRearPivotObj.transform;
            if (axleRearObj)
            {
                mvMotorcycleRearAxle rearAxle = axleRearObj.AddComponent<mvMotorcycleRearAxle>();
                rearAxle.wheelModel = wheelRearObj.transform;

                wheelColliderRear.rearAxle = rearAxle;
            }

            if (shockAbsorberRearObj)
            {
                mvMotorcycleShockAbsorber shockAbsorber = shockAbsorberRearObj.AddComponent<mvMotorcycleShockAbsorber>();
                shockAbsorber.wheelModel = wheelRearObj.transform;

                wheelColliderRear.rearShockAbsorber = shockAbsorber;
            }


#if MIS_SWIMMING
            GameObject divingWaterSplashFXPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                        Path.Combine(MISFeature.MIS_SWIMMING_PATH, "Runtime/FX/DivingWaterSplash/FX_DivingWaterSplash.prefab"));
            vc.waterSplashFXPrefab = divingWaterSplashFXPrefab;
#endif
        }
#endif
    }
}