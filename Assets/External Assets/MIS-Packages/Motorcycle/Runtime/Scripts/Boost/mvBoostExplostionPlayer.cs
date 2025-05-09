using System.Collections;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [mvClassHeader("Boost Explostion Player", iconName = "misIconRed")]
    public class mvBoostExplostionPlayer : mvMonoBehaviour
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        public mvParticlePlayer[] boostExplosionPlayers;
        public float interval = 0.5f;

        // ----------------------------------------------------------------------------------------------------
        // 
        mvMotorcycleInput vcInput;
        bool trigger = false;
        Coroutine coroutine = null;
        WaitForSeconds waitInterval;

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Start()
        {
            vcInput = GetComponentInParent<mvMotorcycleInput>();

            if (vcInput)
            {
                waitInterval = new WaitForSeconds(interval);

                vcInput.onUpdate -= OnUpdate;
                vcInput.onUpdate += OnUpdate;

                vcInput.vc.onDead.RemoveListener(OnDead);
                vcInput.vc.onDead.AddListener(OnDead);
            }
            else
            {
                this.enabled = false;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void OnUpdate(float deltaTime)
        {
            if (!vcInput.vc.isEngineOn)
                return;

            if (vcInput.vc.boostInput && vcInput.vc.localVelocityZ > 0f && !trigger)
            {
                if (coroutine != null)
                    return;

                trigger = true;

                coroutine = StartCoroutine(Play());
            }
            else if (!vcInput.vc.boostInput)
            {
                trigger = false;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        IEnumerator Play()
        {
            for (int i = 0; i < boostExplosionPlayers.Length; i++)
            {
                boostExplosionPlayers[i].Play();
                yield return waitInterval;
            }

            coroutine = null;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void OnDead(GameObject player)
        {
            vcInput.onUpdate -= OnUpdate;

            StopAllCoroutines();

            Destroy(this.gameObject);
        }
#endif
    }
}