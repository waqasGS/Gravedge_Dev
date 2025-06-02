using UnityEngine;
namespace Invector
{
    [vClassHeader("Frame Limiter", false)]
    public class vFrameLimiter : vMonoBehaviour
    {
        public int desiredFPS = 60;

        void Awake()
        {
            QualitySettings.vSyncCount = 0; // Don't sync to refresh rate
            //Application.targetFrameRate = 120;
        }

#if UNITY_EDITOR

        //void Update()
        //{
        //    long lastTicks = DateTime.Now.Ticks;
        //    long currentTicks = lastTicks;
        //    float delay = 1f / desiredFPS;
        //    float elapsedTime;

        //    if (desiredFPS <= 0)
        //        return;

        //    while (true)
        //    {
        //        currentTicks = DateTime.Now.Ticks;
        //        elapsedTime = (float)TimeSpan.FromTicks(currentTicks - lastTicks).TotalSeconds;
        //        if (elapsedTime >= delay)
        //        {
        //            break;
        //        }
        //    }
        //}

#endif
    }
}