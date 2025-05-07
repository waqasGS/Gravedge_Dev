using System.Collections.Generic;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [mvClassHeader("Motorcycle Wheel", iconName = "misIconRed")]
    public class mvMotorcycleWheel : mvMonoBehaviour
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Settings", order = 0)]
        [Header("Wheel")]
        public Transform wheelModel;
        [Tooltip("Please adjust this value for Skidmark width.")]
        public float width = 0.275f;
        public ConfigureVehicleSubsteps cvs = new ConfigureVehicleSubsteps();
        [HideInInspector] public WheelCollider wheelCollider;
        [HideInInspector] public Collider wheelModelCollider;

        [Header("Suspension")]
        public Transform suspension;
        public float suspensionExpensionSpeed = 2f;

        [Header("Controll")]
        public bool isFrontWheel = false;
        public bool hasSteer = false;
        public bool hasPower = true;
        public bool hasBrake = true;
        [Tooltip("If isFrontWheel is true, this value will be set to false by force.")]
        public bool hasHandbrake = false;

        [Header("Friction")]
        public bool useExtreamDrift = false;
        protected WheelFrictionCurve forwardFrictionCurve;
        protected WheelFrictionCurve sidewaysFrictionCurve;
        protected float wheelSlipAmountForward = 0f;
        protected float wheelSlipAmountSideways = 0f;
        protected const float MAX_SLIP = 0.15f;

        public float minForwardStiffness = 0.75f;
        public float maxForwardStiffness = 1f;
        public float minSidewaysStiffness = 0.75f;
        public float maxSidewaysStiffness = 1f;

        [Header("Skidmark")]
        public GameObject skidmarkManagerPrefab;
        protected mvSkidmarkManager skidmarkManager;
        protected int lastSkidmark = -1;

        [Header("Wheel Smoke")]
        internal List<mvParticleEmitter> wheelSmokeEmitterList = new List<mvParticleEmitter>();

        // Audio
        AudioSource audioSource;
        AudioClip audioClip;
        float audioVolume = 0.8f;

        // Ground Data: To-Do
        //TerrainData terrainData;
        //int alphamapWidth;
        //int alphamapHeight;
        //float[,,] splatmapData;
        //float textureCount;


        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Rear Axle", order = 1)]
        [vHelpBox("Set Rear Axle and ShockAbsorber reference only if this wheel is a rear one.")]
        public mvMotorcycleRearAxle rearAxle;
        public mvMotorcycleShockAbsorber rearShockAbsorber;


        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Debug", order = 10)]
        public bool debugMode = false;
        public float debugMotorTorque;
        public float debugBrakeTorque;
        public float debugSteer;
        public bool isGrounded = false;
        public int groundIndex = 0;
        public float totalSlip;


        // ----------------------------------------------------------------------------------------------------
        // 
        [HideInInspector] public mvMotorcycleBase vc;
        public WheelHit wheelHit;
        protected Vector3 wheelPosition;
        protected Quaternion wheelRotation; 
        
        Vector3 suspensionLocalPosition;


        // ----------------------------------------------------------------------------------------------------
        // 
        bool _wheelColliderEnable;
        public virtual bool WheelColliderEnable
        {
            get
            {
                return _wheelColliderEnable;
            }
            set
            {
                if (_wheelColliderEnable != value)
                {
                    _wheelColliderEnable = value;

                    if (_wheelColliderEnable)
                    {
                        wheelCollider.enabled = true;
                        wheelModelCollider.enabled = false;
                    }
                    else
                    {
                        wheelCollider.enabled = false;
                        wheelModelCollider.enabled = true;
                    }
                }
            }
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            vc = GetComponentInParent<mvMotorcycleBase>();

            wheelCollider = GetComponent<WheelCollider>();
            wheelCollider.ConfigureVehicleSubsteps(cvs.speedThreshold, cvs.stepsBelowThreshold, cvs.stepsAboveThreshold);

            wheelModelCollider = wheelModel.GetComponent<Collider>();
            WheelColliderEnable = true;

            if (suspension)
                suspensionLocalPosition = suspension.localPosition;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Start()
        {
            wheelCollider.mass = vc.rb.mass / vc.allWheelCount * vc.wheelColliderMassRatio;

            if (isFrontWheel)
                hasHandbrake = false;

            SetWheelFrictionData();
            SetSkidmarkManager();
            SetWheelSmokePlayer();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void SetWheelFrictionData()
        {
            if (vc.wheelFrictionData)
            {
                forwardFrictionCurve.extremumSlip = vc.wheelFrictionData.forwardExtremumSlip;
                forwardFrictionCurve.extremumValue = vc.wheelFrictionData.forwardExtremumValue;
                forwardFrictionCurve.asymptoteSlip = vc.wheelFrictionData.forwardAsymptoteSlip;
                forwardFrictionCurve.asymptoteValue = vc.wheelFrictionData.forwardAsymptoteValue;

                sidewaysFrictionCurve.extremumSlip = vc.wheelFrictionData.sidewaysExtremumSlip;
                sidewaysFrictionCurve.extremumValue = vc.wheelFrictionData.sidewaysExtremumValue;
                sidewaysFrictionCurve.asymptoteSlip = vc.wheelFrictionData.sidewaysAsymptoteSlip;
                sidewaysFrictionCurve.asymptoteValue = vc.wheelFrictionData.sidewaysAsymptoteValue;
            }
            else
            {
                forwardFrictionCurve = wheelCollider.forwardFriction;
                sidewaysFrictionCurve = wheelCollider.sidewaysFriction;
            }

            wheelCollider.forwardFriction = forwardFrictionCurve;
            wheelCollider.sidewaysFriction = sidewaysFrictionCurve;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void SetSkidmarkManager()
        {
            if (skidmarkManagerPrefab != null)
            {
                GameObject skidmarkManagerInstance = Instantiate(skidmarkManagerPrefab);
                skidmarkManager = skidmarkManagerInstance.GetComponent<mvSkidmarkManager>();
                skidmarkManager.Initialize(vc.groundData);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void SetWheelSmokePlayer()
        {
            wheelSmokeEmitterList.Clear();

            for (int i = 0; i < vc.groundData.groundFrictions.Length; i++)
            {
                GameObject ps = Instantiate(vc.groundData.groundFrictions[i].slipSmokePrefab);
                ps.transform.SetParent(transform, false);
                ps.transform.localPosition = Vector3.zero;
                ps.transform.localRotation = Quaternion.identity;
                wheelSmokeEmitterList.Add(ps.GetComponent<mvParticleEmitter>());
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateWheelCollider(float deltaTime)
        {
            isGrounded = wheelCollider.GetGroundHit(out wheelHit);
            groundIndex = transform.GetGroundIndex(wheelHit, vc.groundData);

            UpdateFrictions();
            UpdateSkidMarks(deltaTime);
            UpdateAudio();
            UpdateWheelSmoke();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateWheelAlignment(float deltaTime)
        {
            wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);

            wheelModel.Rotate(wheelCollider.rpm / 60f * 360f * deltaTime, 0, 0);

            if (suspension == null)
                return;

            if (wheelCollider.GetGroundHit(out wheelHit))
            {
                float distance = 
                    Vector3.Distance(wheelCollider.transform.position - (wheelCollider.transform.up * wheelCollider.suspensionDistance * 0.5f), wheelPosition);
                suspension.localPosition = new Vector3(suspensionLocalPosition.x, suspensionLocalPosition.y + distance, suspensionLocalPosition.z);
            }
            else
            {
                suspension.localPosition = Vector3.Lerp(suspension.localPosition, suspensionLocalPosition, suspensionExpensionSpeed * deltaTime);
            }

            if (rearAxle)
                rearAxle.UpdateRotation();

            if (rearShockAbsorber)
                rearShockAbsorber.UpdateScale();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateFrictions()
        {
            vc.groundData.groundFrictions[groundIndex].UpdateFrictionCurve(isFrontWheel, vc.handBrakeInput, maxForwardStiffness, maxSidewaysStiffness, ref forwardFrictionCurve, ref sidewaysFrictionCurve);

            if (useExtreamDrift)
                UpdateDrift();

            wheelCollider.forwardFriction = forwardFrictionCurve;
            wheelCollider.sidewaysFriction = sidewaysFrictionCurve;
            wheelCollider.wheelDampingRate = vc.groundData.groundFrictions[groundIndex].wheelDampingRate;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateSkidMarks(float deltaTime)
        {
            if (isGrounded)
            {
                wheelSlipAmountForward = Mathf.Abs(wheelHit.forwardSlip);
                wheelSlipAmountSideways = Mathf.Abs(wheelHit.sidewaysSlip);
            }
            else
            {
                wheelSlipAmountForward = 0f;
                wheelSlipAmountSideways = 0f;
            }

            totalSlip = Mathf.Lerp(totalSlip, ((wheelSlipAmountSideways + wheelSlipAmountForward) * 0.5f), deltaTime * 5f);

            if (totalSlip > vc.groundData.groundFrictions[groundIndex].slip)
            {
                Vector3 point = wheelHit.point + 2f * vc.rb.velocity * deltaTime;

                if (vc.velocityMagnitude > 1f)
                    lastSkidmark = skidmarkManager.AddSkidMark(point, wheelHit.normal, totalSlip - vc.groundData.groundFrictions[groundIndex].slip, lastSkidmark, groundIndex, width);
                else
                    lastSkidmark = -1;
            }
            else
            {
                lastSkidmark = -1;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateDrift()
        {
            float extremumValue, asymptoteValue;
            float relativeSlipVelocity = transform.GetRelativeSlipVelocity(vc.rb.velocity, wheelHit);

            transform.GetDriftForwardFriction(isFrontWheel, relativeSlipVelocity, minForwardStiffness, maxForwardStiffness, out extremumValue, out asymptoteValue);
            forwardFrictionCurve.extremumValue = extremumValue;
            forwardFrictionCurve.asymptoteValue = asymptoteValue;

            transform.GetDriftSidewaysFriction(isFrontWheel, relativeSlipVelocity, minForwardStiffness, maxForwardStiffness, out extremumValue, out asymptoteValue);
            sidewaysFrictionCurve.extremumValue = extremumValue;
            sidewaysFrictionCurve.asymptoteValue = asymptoteValue;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateAudio()
        {
            if (totalSlip > vc.groundData.groundFrictions[groundIndex].slip)
            {
                audioClip = vc.groundData.groundFrictions[groundIndex].acSkid;
                audioVolume = vc.groundData.groundFrictions[groundIndex].volume;

                if (audioSource.clip != audioClip)
                    audioSource.clip = audioClip;

                if (!audioSource.isPlaying)
                    audioSource.Play();

                if (vc.velocityMagnitude > 1f)
                {
                    audioSource.volume = Mathf.Lerp(0f, audioVolume, totalSlip);
                    audioSource.pitch = Mathf.Lerp(1f, 0.8f, audioSource.volume);
                }
                else
                {
                    audioSource.volume = 0f;
                }
            }
            else
            {
                audioSource.volume = 0f;

                if (audioSource.volume <= 0.05f && audioSource.isPlaying)
                    audioSource.Stop();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateWheelSmoke()
        {
            for (int i = 0; i < wheelSmokeEmitterList.Count; i++)
            {
                if (totalSlip > vc.groundData.groundFrictions[groundIndex].slip)
                {
                    if (i != groundIndex)
                        wheelSmokeEmitterList[i].EmitMultiply(0);
                    else
                        wheelSmokeEmitterList[i].EmitMultiply(1);
                }
                else
                {
                    wheelSmokeEmitterList[i].EmitMultiply(0);
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ApplySteering(float steerInput, float angle, bool isSingle = false)
        {
#if UNITY_EDITOR
            if (debugMode)
                debugSteer = angle;
#endif

            if (steerInput == 0f)
                wheelCollider.steerAngle = 0f;
            else
                wheelCollider.steerAngle = angle;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ApplyBrakeTorque(float torque)
        {
#if UNITY_EDITOR
            if (debugMode)
                debugBrakeTorque = torque;
#endif

            wheelCollider.brakeTorque = torque;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ApplyMotorTorque(float torque)
        {
#if UNITY_EDITOR
            if (debugMode)
                debugMotorTorque = torque;
#endif

            wheelCollider.motorTorque = torque;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        [System.Serializable]
        public class ConfigureVehicleSubsteps
        {
            public float speedThreshold = 10f;
            public int stepsBelowThreshold = 5;
            public int stepsAboveThreshold = 5;
        }
#endif
    }
}