using Invector;
using System.Collections.Generic;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("Motorcycle", iconName = "misIconRed")]
    public class mvMotorcycle : mvMotorcycleBase
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Vehicle", order = 2)]
        [Header("Wheel")]
        public List<mvMotorcycleWheel> wheelList = new List<mvMotorcycleWheel>();

        [Header("Leaning")]
        public float leaningSpeed = 5f;
        protected float leaningAngle;
        protected float leaningRotate;
        protected Quaternion leaningRotateBody;
        protected Quaternion leaningRotateRigidbody;

        [Header("Wheelie")]
        public mvFloatOrigin wheelieAngle = new mvFloatOrigin(30f);
        public float wheelieSpeed = 10f;

        // ----------------------------------------------------------------------------------------------------
        // 
        [Header("UI")]
        public GameObject vehicleGlassPanel;
        public bool useSpeedometer = true;
        mvSpeedometer speedometer;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void Awake()
        {
            base.Awake();

            allWheelCount = wheelList.Count;

            wheelieAngle.now = 0f;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void Start()
        {
            base.Start();

            if (useSpeedometer)
            {
                speedometer = GetComponentInChildren<mvSpeedometer>();
                speedometer.InitilaizeMaxHealth(MaxHealth);
                speedometer.InitilaizeMaxStamina(maxStamina.origin);
                speedometer.InitilaizeEngineRPM(engineRPM.min, engineRPM.max);

                onChangeMaxHealth.AddListener(speedometer.OnChangeMaxHealth);
                onChangeHealth.AddListener(speedometer.OnChangedHealth);

                onChangeMaxStamina.AddListener(speedometer.OnChangeMaxStamina);
                onChangeStamina.AddListener(speedometer.OnChangeStamina);
            }
        }

        /*
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void Init()
        {
            base.Init();


        }*/

        // ----------------------------------------------------------------------------------------------------
        // FixedUpdate
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateMotor(float deltaTime)
        {
            if (!IsAvailable)
                return;

            CheckOverturned();

            if (!isEngineOn)
                return;

            CheckHealth();
            UpdateBoostStamina(deltaTime);

            UpdateEngine(deltaTime);
            for (int i = 0; i < wheelList.Count; i++)
                wheelList[i].UpdateWheelCollider(deltaTime);
            UpdateWheel(deltaTime);

            CheckGround();
            CheckESP();
            UpdateDownForce();

            UpdateAirControl(deltaTime);

            StaminaRecovery(deltaTime);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateEngine(float deltaTime)
        {
            // Speed
            velocityMagnitude = rb.velocity.magnitude;
            maxSpeed.now = velocityMagnitude * 3.6f;

            localVelocityZ = transform.InverseTransformDirection(rb.velocity).z;
            speedProportion = Mathf.InverseLerp(0, localVelocityZ > 0f ? maxSpeed.max : maxSpeed.min, maxSpeed.now);

            // Wheel RPM
            wheelRPM = 0f;
            powerWheelCount = 0;

            for (int i = 0; i < wheelList.Count; i++)
            {
                if (wheelList[i].hasPower)
                {
                    wheelRPM += wheelList[i].wheelCollider.rpm;
                    powerWheelCount++;
                }
            }

            if (isEngineOn)
            {
                if (gear >= 0f)
                    engineRPM.now = Mathf.Lerp(engineRPM.min, engineRPM.max, throttleInputSmooth);
                else
                    engineRPM.now = Mathf.Lerp(engineRPM.min, engineRPM.max, -throttleInputSmooth);

                rpmProportion = Mathf.InverseLerp(engineRPM.min, engineRPM.max, engineRPM.now);
            }
            else
            {
                engineRPM.now = Mathf.Lerp(engineRPM.now, 0f, engineRPMInertia * deltaTime);

                rpmProportion = Mathf.InverseLerp(0f, engineRPM.max, engineRPM.now);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateWheel(float deltaTime)
        {
            leaningAngle = Mathf.LerpAngle(leaningAngle, speedProportion * -steeringAngle.now, deltaTime * leaningSpeed);

            if (isDead)
                return;

            leaningRotate = (transform.eulerAngles.z > 90f && transform.eulerAngles.z < 270f) ? 180f : 0f;
            wheelieAngle.now = Mathf.Clamp(wheelieAngle.now, 0f, wheelieAngle.origin);
            //wheelieAngle.now = Mathf.MoveTowards(wheelieAngle.now, 0f, wheelieSpeed * deltaTime);

            leaningRotateRigidbody = Quaternion.Euler(0f, 0f, leaningRotate - transform.localEulerAngles.z);
            rb.MoveRotation(rb.rotation * leaningRotateRigidbody);

            leaningRotateBody = Quaternion.Euler(-wheelieAngle.now, 0f, leaningRotate - transform.localEulerAngles.z + leaningAngle);
            bodyTransform.localRotation = leaningRotateBody;

            for (int i = 0; i < wheelList.Count; i++)
            {
                mvMotorcycleWheel bikeWheel = wheelList[i];

                if (isDead)
                {
                    bikeWheel.WheelColliderEnable = false;
                    continue;
                }
                bikeWheel.WheelColliderEnable = true;

                // Steering
                if (bikeWheel.hasSteer)
                    bikeWheel.ApplySteering(input.x, steeringAngle.now);
                else
                    bikeWheel.ApplySteering(0f, 0f);

                bikeWheel.UpdateWheelAlignment(deltaTime);

                // Brake
                bool hasBrakeTorque = false;

                if (handBrakeInput && bikeWheel.hasHandbrake)
                {
                    hasBrakeTorque = true;
                    bikeWheel.ApplyBrakeTorque(maxBrakeTorque);
                }

                if (brakeInput >= 0.2f && bikeWheel.hasBrake && !hasBrakeTorque)
                {
                    if (brakeInput < 0.2f)
                        engineSound.PlayOneShot(AudioSourceType.Brake); // One time sound

                    hasBrakeTorque = true;
                    bikeWheel.ApplyBrakeTorque(brakeInput * maxBrakeTorque);
                }

                if (!hasBrakeTorque || !bikeWheel.hasBrake)
                    bikeWheel.ApplyBrakeTorque(0f);

                // Motor
                if (bikeWheel.hasPower)
                {
                    if (hasBrakeTorque)
                    {
                        bikeWheel.ApplyMotorTorque(0f);
                    }
                    else
                    {
#if MIS_INVECTOR_SWIMMING
                        if (isUnderWater)
                        {
                            bikeWheel.ApplyMotorTorque(0f);
                        }
                        else
#endif
                        {
#if MIS_INVECTOR_SWIMMING
                            if (inTheWater)
                            {
                                if (!boostInput
                                    && (gear > 0 && maxSpeed.now > maxSpeed.max * inWaterSpeedMultiplier) || (gear < 0 && maxSpeed.now > maxSpeed.min * inWaterSpeedMultiplier))
                                {
                                    bikeWheel.ApplyMotorTorque(0f);

                                    if (!isJumping)
                                        rb.velocity = Vector3.Lerp(rb.velocity, ClampMaxVelocityBySpeed(rb.velocity, gear > 0 ? maxSpeed.max : maxSpeed.min), engineRPMInertia * deltaTime);
                                }
                                else if (/*boostInput && */(gear > 0 && maxSpeed.now > boostSpeed.max * inWaterSpeedMultiplier) || (gear < 0 && maxSpeed.now > boostSpeed.min * inWaterSpeedMultiplier))
                                {
                                    bikeWheel.ApplyMotorTorque(-10f);

                                    //if (!isJumping)
                                    //    rb.velocity = ClampMaxVelocityBySpeed(rb.velocity, gear > 0 ? boostSpeed.max : boostSpeed.min);
                                }
                                else
                                {
                                    bikeWheel.ApplyMotorTorque(input.z/*throttleInputSmooth*/ * (boostInput && !bikeWheel.isFrontWheel ? boostMultiplier : 1f) * maxMotorTorque);
                                }
                            }
                            else
#endif
                            {
                                if (!boostInput
                                    && (gear > 0 && maxSpeed.now > maxSpeed.max) || (gear < 0 && maxSpeed.now > maxSpeed.min))
                                {
                                    bikeWheel.ApplyMotorTorque(0f);

                                    if (!isJumping)
                                        rb.velocity = Vector3.Lerp(rb.velocity, ClampMaxVelocityBySpeed(rb.velocity, gear > 0 ? maxSpeed.max : maxSpeed.min), engineRPMInertia * deltaTime);
                                }
                                else if (/*boostInput && */(gear > 0 && maxSpeed.now > boostSpeed.max) || (gear < 0 && maxSpeed.now > boostSpeed.min))
                                {
                                    bikeWheel.ApplyMotorTorque(-10f);

                                    //if (!isJumping)
                                    //    rb.velocity = ClampMaxVelocityBySpeed(rb.velocity, gear > 0 ? boostSpeed.max : boostSpeed.min);
                                }
                                else
                                {
                                    bikeWheel.ApplyMotorTorque(input.z/*throttleInputSmooth*/ * (boostInput && !bikeWheel.isFrontWheel ? boostMultiplier : 1f) * maxMotorTorque);
                                }
                            }
                        }
                    }
                }
            }

            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void CheckGround()
        {
            bool grounded = true;

            if (isDead || isDisabledCheckGround)
            {
                isGrounded = true;
                heightReached = transform.position.y;
                return;
            }

            for (int i = 0; i < wheelList.Count; i++)
            {
                if (!wheelList[i].wheelCollider.isGrounded)
                {
                    grounded = false;
                    break;
                }
            }

            isGrounded = grounded;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void CheckESP()
        {
            frontSlip = wheelList[0].wheelHit.sidewaysSlip;
            rearSlip = wheelList[1].wheelHit.sidewaysSlip;

            IsUnderSteering = Mathf.Abs(frontSlip) >= ESPThreshold;
            IsOverSteering = Mathf.Abs(rearSlip) >= ESPThreshold;

            if (IsUnderSteering || IsOverSteering)
                isESPWorking = true;
            else
                isESPWorking = false;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void CheckOverturned()
        {
            if (transform.up.y < -overturnedAngle * 0.01)
            {
                if (rider && !isOverturned)
                {
                    rider.ExitEntry(true);
                    rider.ExitActionState(true);

                    // WIP
                    //?Invoke("AutoRecoverOverturned", recoverOverturnedDelay);
                }

                isOverturned = true;
            }
            else
            {
                isOverturned = false;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void AutoRecoverOverturned()
        {
            transform.SetLocalEulerX(0f);
            transform.SetLocalEulerZ(0f);
        }

        // ----------------------------------------------------------------------------------------------------
        // Update
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateInput(Vector3 input, float deltaTime)
        {
            if (!IsAvailable || isDead)
                return;

#if MIS_INVECTOR_SWIMMING
            if (water)
            {
                waterHeightLevel = water.transform.position.y;
                curretVehicleDepth = -(VehicleCenter.y - waterHeightLevel);
                isUnderWater = curretVehicleDepth > 0f;

                if (isUnderWater)
                {
                    rider.ExitByForce(false);
                    IsAvailable = false;
                    return;
                }
            }
#endif

            this.input = input;

            // Throttle Input
            throttleInputSmooth = Mathf.Lerp(throttleInputSmooth, input.z, throttleSmoothDamp * deltaTime);
            throttleInputAbs = Mathf.Abs(input.z);

            // Steer Input
            steerInputAbs = Mathf.Abs(input.x);

            // Brake Input
            if (input.z < -0.2f)
            {
                if (localVelocityZ > 0.5f)
                    brakeInput = throttleInputAbs;
                else
                    brakeInput = 0f;
            }
            else if (input.z > 0.2f)
            {
                if (localVelocityZ < -0.5f)
                    brakeInput = throttleInputAbs;
                else
                    brakeInput = 0f;
            }

            moveDirection = input.x * transform.right + input.z/*throttleInputSmooth*/ * transform.forward;

            UpdateSteeringWheel(deltaTime);
            UpdateGear(deltaTime);
            UpdateUI(deltaTime);

            if (isEngineOn)
            {
                //? when !isEngineOn, all engine sound must be stop!
                if (gear >= 0)
                {
                    engineSound.UpdateSound(rpmProportion, false);
                    engineSound.PlaySound(AudioSourceType.ReverseSound, false);
                }
                else
                {
                    engineSound.PlaySound(AudioSourceType.ReverseSound, true);
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        //protected override void UpdateGear(float deltaTime)
        //{
        //}

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        //protected override void SetGear(int targetGear)
        //{
        //}

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateSteeringWheel(float deltaTime)
        {
            steeringAngle.now = Mathf.LerpAngle(steeringAngle.now, input.x * steeringAngle.origin, handBrakeInput ? steeringSmoothDamp.max : steeringSmoothDamp.min);

            if (steeringWheel == null)
                return;

            switch (steeringWheelAxis)
            {
            case Axis.X:
                steeringWheel.localRotation = orginSteeringWheelRotation * Quaternion.Euler(steeringAngle.now, 0f, 0f);
                break;
            case Axis.Y:
                steeringWheel.localRotation = orginSteeringWheelRotation * Quaternion.Euler(0f, steeringAngle.now, 0f);
                break;
            case Axis.Z:
                steeringWheel.localRotation = orginSteeringWheelRotation * Quaternion.Euler(0f, 0f, steeringAngle.now);
                break;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateUI(float deltaTime)
        {
            if (!useSpeedometer || !isEngineOn)
                return;

            speedometer.SetSpeed((int)maxSpeed.now, engineRPM.now, gear);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void CheckHealth()
        {
            if (isDead)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.centerOfMass = Vector3.zero;

                SetGear(0);

                EngineOff();
                OnActiveRagdoll();

                //RemoveComponents();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void GetOn()
        {
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void GetOff()
        {
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void EngineOn()
        {
            if (isEngineOn)
                return;
            isEngineOn = true;

            engineSound.StartEngineSound();

            if (useSpeedometer)
                speedometer.DisplaySpeedometer(true);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void EngineOff()
        {
            if (!isEngineOn)
                return;
            isEngineOn = false;

            engineSound.StopEngineSound();

            if (useSpeedometer)
                speedometer.DisplaySpeedometer(false);
        }

        // ----------------------------------------------------------------------------------------------------
        // Called after EngineOn()
        // ----------------------------------------------------------------------------------------------------
        public virtual void EnterActionState(mvMotorcycleRider rider)
        {
            triggerActionState = true;
            OnStartAction.Invoke(rider.gameObject);
            IsOnAction = true;

            this.rider = rider;

            if (stabilizerCoroutine != null)
            {
                StopCoroutine(stabilizerCoroutine);
                stabilizerCoroutine = null;
            }
            rb.isKinematic = false;

            if (engineSound.acEngineStart)
            {
                engineSound.PlayOneShot(AudioSourceType.EngineStart);
                Invoke("EngineOn", engineSound.acEngineStart.length);
            }
            else
            {
                EngineOn();
            }

            if (vehicleGlassPanel)
                vehicleGlassPanel.SetActive(true);

#if MIS_VEHICLEWEAPONS
            if (gunLauncher)
                gunLauncher.SetEnable(transform, rb, true);
            if (rocketLauncher)
                rocketLauncher.SetEnable(transform, rb, true);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // Called after EngineOff()
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExitActionState(bool byRagdoll)
        {
            if (!triggerActionState)
                return;
            triggerActionState = false;
            OnFinishAction.Invoke(this.rider.gameObject);
            IsOnAction = false;

            //if (!byRagdoll)
            //    this.rider = null;
            this.rider = null;

            if (stabilizerCoroutine == null)
                stabilizerCoroutine = StartCoroutine(VehicleStabilizer(
                    delegate
                    {
                        rb.isKinematic = true;
                    }));

            if (engineSound.acEngineStop)
            {
                engineSound.PlayOneShot(AudioSourceType.EngineStop);
                Invoke("EngineOff", engineSound.acEngineStop.length);
            }
            else
            {
                EngineOff();
            }

            if (vehicleGlassPanel)
                vehicleGlassPanel.SetActive(false);

#if MIS_VEHICLEWEAPONS
            if (gunLauncher)
                gunLauncher.SetEnable(transform, rb, false);
            if (rocketLauncher)
                rocketLauncher.SetEnable(transform, rb, false);
#endif
        }
#endif
    }
}