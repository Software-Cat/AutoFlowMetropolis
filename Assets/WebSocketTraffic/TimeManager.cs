using UnityEngine;

namespace WebSocketTraffic
{
    public class TimeManager : MonoBehaviour
    {
        public float timeScale = 1;

        private void Start()
        {
            Time.timeScale = timeScale;
        }
    }
}