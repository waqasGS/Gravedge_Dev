using Invector;
using UnityEngine;

namespace com.mobilin.games
{
	// ----------------------------------------------------------------------------------------------------
	// 
	// ----------------------------------------------------------------------------------------------------
	[vClassHeader("Motorcycle Rear Axle", iconName = "misIconRed")]
	public class mvMotorcycleRearAxle : vMonoBehaviour
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


		// ----------------------------------------------------------------------------------------------------
		// 
		// ----------------------------------------------------------------------------------------------------
		protected virtual void Awake()
        {
			if (hasPivotParent)
				pivot = transform.parent;
			else
				pivot = transform;
		}

		// ----------------------------------------------------------------------------------------------------
		// 
		// ----------------------------------------------------------------------------------------------------
		protected virtual void Start()
		{
			isOnAction = body != null;
		}

		// ----------------------------------------------------------------------------------------------------
		// 
		// ----------------------------------------------------------------------------------------------------
		public virtual void UpdateRotation()
		{
			if (!isOnAction)
				return;

			pivot.rotation = Quaternion.LookRotation((wheelModel.position + offset) - pivot.position);
			pivot.rotation *= Quaternion.Inverse(body.localRotation);
		}
#endif
	}
}