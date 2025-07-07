using UnityEngine;

namespace Invector.vCharacterController
{
    [System.Serializable]
    public class vLine
    {
        public Vector3 p1;
        public Vector3 p2;
        
        public vLine(Vector3 point1, Vector3 point2)
        {
            p1 = point1;
            p2 = point2;
        }
        
        public void Draw(Color color, float duration = 0.1f, bool draw = true)
        {
            if (draw)
            {
                Debug.DrawLine(p1, p2, color, duration);
            }
        }
    }
}