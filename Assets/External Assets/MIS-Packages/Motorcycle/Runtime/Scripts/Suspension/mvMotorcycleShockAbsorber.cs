using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("Motorcycle ShockAbsorber", iconName = "misIconRed")]
    public class mvMotorcycleShockAbsorber : vMonoBehaviour
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Settings", order = 0)]
        public Transform body;
        public Transform wheelModel;
        public Vector3 offset;
        public bool hasPivotParent = true;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Debug", order = 100)]
        [mvReadOnly] [SerializeField] protected bool isOnAction;


        // ----------------------------------------------------------------------------------------------------
        // 
        protected Transform pivot = null;
        protected float originDistance;


        // ----------------------------------------------------------------------------------------------------
        // 
        protected virtual Vector3 WheelPoint
        {
            get => wheelModel.TransformPoint(offset);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        protected virtual float WheelDistance
        {
            get => (pivot.position - WheelPoint).magnitude;
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Awake()
        {
            if (hasPivotParent)
                pivot = transform.parent;
            else
                pivot = transform;

            originDistance = WheelDistance;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Start()
        {
            isOnAction = body != null && wheelModel != null;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateScale()
        {
            if (!isOnAction)
                return;

            pivot.SetLocalScaleZ(WheelDistance / originDistance);

            pivot.rotation = Quaternion.LookRotation((wheelModel.position + offset) - pivot.position);
            pivot.rotation *= Quaternion.Inverse(body.localRotation);
        }
#endif
    }
}