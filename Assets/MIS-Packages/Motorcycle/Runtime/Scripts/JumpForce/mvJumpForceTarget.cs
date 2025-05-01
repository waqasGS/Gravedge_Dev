using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("JumpForce Target", iconName = "misIconRed")]
    public class mvJumpForceTarget : vMonoBehaviour
    {
        public enum ForceTargetType
        {
            CenterOfMass = 0,
            Pivot,
            RendererBoundsCenter
        };
        public ForceTargetType windStormApplyTarget = ForceTargetType.CenterOfMass;

        // ----------------------------------------------------------------------------------------------------
        // 
        Rigidbody targetRigidbody;
        Renderer targetRenderer;

        // ----------------------------------------------------------------------------------------------------
        // 
        public Vector3 Position
        {
            get
            {
                switch (windStormApplyTarget)
                {
                case ForceTargetType.Pivot:
                    return transform.position;
                case ForceTargetType.CenterOfMass:
                    return targetRigidbody.worldCenterOfMass;
                case ForceTargetType.RendererBoundsCenter:
                    return targetRenderer.bounds.center;
                default:
                    return transform.position;
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Start()
        {
            TryGetComponent(out targetRigidbody);
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void WindStorm(Vector3 origin, Vector3 direction, float power)
        {
            targetRigidbody.AddForceAtPosition(direction * power, origin);
        }
    }
}