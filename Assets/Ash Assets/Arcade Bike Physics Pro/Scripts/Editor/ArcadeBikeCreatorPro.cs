using UnityEditor;
using UnityEngine;

namespace ArcadeBP_Pro
{
    public class ArcadeBikeCreatorPro : EditorWindow
    {
        GameObject preset;
        Transform BikeParent;
        Transform Handle;
        Transform frontWheel;
        Transform backWheel;

        MeshRenderer bodyMesh;
        MeshRenderer frontWheelMesh;
        MeshRenderer backWheelMesh;

        private GameObject NewBike;

        [MenuItem("Tools/Ash Tools/Arcade Bike Physics Pro/Bike Creator")]
        static void OpenWindow()
        {
            ArcadeBikeCreatorPro ArcadeBikeCreatorProWindow = (ArcadeBikeCreatorPro)GetWindow(typeof(ArcadeBikeCreatorPro));
            ArcadeBikeCreatorProWindow.minSize = new Vector2(400, 350);
            ArcadeBikeCreatorProWindow.Show();
        }

        [MenuItem("Tools/Ash Tools/Arcade Bike Physics Pro/Online Documentation")]
        private static void OpenDocumentationLink()
        {
            string url = "https://soft-pilot-e91.notion.site/Documentation-40c4b6d5135b4fa080694e38d8a1b1d3";
            Application.OpenURL(url);
        }

        private void OnGUI()
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = Color.green;

            GUILayout.Label("Arcade Bike Creator Pro", style);
            preset = EditorGUILayout.ObjectField("Bike preset", preset, typeof(GameObject), true) as GameObject;
            ValidateBikePreset(preset);

            GUILayout.Label("Your Bike", style);
            BikeParent = EditorGUILayout.ObjectField("Bike Parent", BikeParent, typeof(Transform), true) as Transform;
            ValidatePrefabStatus();
            ValidateBikeParentOrientation();

            Handle = EditorGUILayout.ObjectField("Handle", Handle, typeof(Transform), true) as Transform;
            ValidateHandleOrientation();

            frontWheel = EditorGUILayout.ObjectField("Wheel Front", frontWheel, typeof(Transform), true) as Transform;
            backWheel = EditorGUILayout.ObjectField("Wheel Back", backWheel, typeof(Transform), true) as Transform;

            bodyMesh = EditorGUILayout.ObjectField("Body Mesh", bodyMesh, typeof(MeshRenderer), true) as MeshRenderer;
            frontWheelMesh = EditorGUILayout.ObjectField("Front Wheel Mesh", frontWheelMesh, typeof(MeshRenderer), true) as MeshRenderer;
            backWheelMesh = EditorGUILayout.ObjectField("Back Wheel Mesh", backWheelMesh, typeof(MeshRenderer), true) as MeshRenderer;


            if (GUILayout.Button("Create Bike"))
            {
                createBike();
            }
        }

        private void createBike()
        {
            NewBike = Instantiate(preset, bodyMesh.bounds.center, BikeParent.rotation);
            NewBike.name = "ABP_Pro_" + BikeParent.name;
            var bikeController = NewBike.GetComponent<ArcadeBikeControllerPro>();
            var bikeRefs = bikeController.bikeReferences;

            NewBike.transform.position = new Vector3(BikeParent.position.x, backWheel.position.y - backWheelMesh.bounds.extents.y, (frontWheel.position.z + backWheel.position.z)/2);

            DestroyImmediate(bikeRefs.BodyMesh.GetChild(0).gameObject);

            BikeParent.SetParent(bikeRefs.BodyMesh);
            BikeParent.SetSiblingIndex(0);

            if (bikeRefs.BikeSteering)
            {
                bikeRefs.BikeSteeringParent.position = Handle.position;
                bikeRefs.BikeSteeringParent.rotation = Handle.rotation;
                GameObject.DestroyImmediate(bikeRefs.SteeringMeshes.GetChild(0).gameObject);
                Handle.SetParent(bikeRefs.SteeringMeshes);
            }

            if (bikeRefs.FrontWheel)
            {
                GameObject.DestroyImmediate(bikeRefs.FrontWheel.GetChild(0).gameObject);
                bikeRefs.FrontWheelParent.position = frontWheel.position;
                bikeRefs.FrontWheelParent.localRotation = Quaternion.identity;
                frontWheel.SetParent(bikeRefs.FrontWheel);
            }
            if (bikeRefs.RearWheel)
            {
                GameObject.DestroyImmediate(bikeRefs.RearWheel.GetChild(0).gameObject);
                bikeRefs.RearWheelParent.position = backWheel.position;
                backWheel.SetParent(bikeRefs.RearWheel);
            }

            var bikeGeometry = bikeController.bikeGeometry;

            var temp_f_rot = frontWheelMesh.transform.rotation;
            var temp_b_rot = backWheelMesh.transform.rotation;
            frontWheelMesh.transform.rotation = Quaternion.identity;
            backWheelMesh.transform.rotation = Quaternion.identity;

            bikeGeometry.FrontWheelRadius = frontWheelMesh.bounds.extents.y;
            bikeGeometry.RearWheelRadius = backWheelMesh.bounds.extents.y;
            bikeGeometry.FrontWheelWidth = frontWheelMesh.bounds.extents.x;
            bikeGeometry.RearWheelWidth = backWheelMesh.bounds.extents.x;
            bikeGeometry.FrontWheelAngle = Vector3.Angle(bikeRefs.FrontWheelParent.up, NewBike.transform.up);
            bikeGeometry.RearWheelAngle = Vector3.Angle(bikeRefs.RearWheelParent.up, NewBike.transform.up);

            frontWheelMesh.transform.rotation = temp_f_rot;
            backWheelMesh.transform.rotation = temp_b_rot;

            // adjust collider
            bikeRefs.collider.transform.position = bodyMesh.bounds.center;
            bikeRefs.collider.transform.localPosition = new Vector3(0, bikeRefs.collider.transform.localPosition.y, bikeRefs.collider.transform.localPosition.z);
            bikeRefs.collider.center = Vector3.zero;
            bikeRefs.collider.height = bodyMesh.bounds.extents.z * 2;
            bikeRefs.collider.radius = bodyMesh.bounds.extents.x;

            DisableMotionBlurForAllChildren(NewBike);
        }


        private void ValidateBikePreset(GameObject bikePreset)
        {
            if (bikePreset != null)
            {
                var bikeController = bikePreset.GetComponent<ArcadeBikeControllerPro>();
                if (bikeController == null)
                {
                    EditorGUILayout.HelpBox("Bike preset does not have an ArcadeBikeControllerPro script attached. Please assign a valid bike preset.", MessageType.Error);

                }
            }

        }


        private void ValidateBikeParentOrientation()
        {
            if (BikeParent != null && frontWheel != null && backWheel != null)
            {
                Vector3 wheelDirection = (frontWheel.position - backWheel.position).normalized;
                if (Vector3.Dot(BikeParent.forward, wheelDirection) < 0 || Vector3.Dot(BikeParent.right, -Vector3.Cross(wheelDirection, Vector3.up)) < 0)
                {
                    EditorGUILayout.HelpBox("Bike Parent orientation is incorrect. Please fix it before creating the bike. Refer to the bike model that comes with this pack", MessageType.Error);
                }

            }
        }

        private void ValidateHandleOrientation()
        {
            if (Handle != null && frontWheel != null && backWheel != null && BikeParent != null)
            {
                Vector3 wheelDirection = (frontWheel.position - backWheel.position).normalized;
                if (Vector3.Dot(Handle.right, -Vector3.Cross(wheelDirection, BikeParent.up)) < 0 || Vector3.Dot(Handle.forward, wheelDirection) < 0)
                {
                    EditorGUILayout.HelpBox("Handle orientation is incorrect. Please fix it before creating the bike. Refer to the bike model that comes with this pack", MessageType.Error);
                }
            }
        }

        private void ValidatePrefabStatus()
        {
            if (BikeParent != null && PrefabUtility.IsPartOfPrefabInstance(BikeParent.gameObject))
            {
                EditorGUILayout.HelpBox("Bike Parent is a prefab instance. Please unpack the prefab completely.", MessageType.Error);
            }
        }

        private void DisableMotionBlurForAllChildren(GameObject parent)
        {
            MeshRenderer[] meshRenderers = parent.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer mr in meshRenderers)
            {
                mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            }
        }
    }
}
