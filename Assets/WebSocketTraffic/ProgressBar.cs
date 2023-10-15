using UnityEngine;
using UnityEngine.UI;

namespace WebSocketTraffic
{
    public class ProgressBar : MonoBehaviour
    {
        public RawImage barImage;
        public RawImage carIcon;
        public float minX;
        public float maxX;

        public float _percent;

        public float Percent
        {
            get => _percent;

            set
            {
                if (value == _percent) return;
                _percent = value;

                // Bar
                var tr = barImage.rectTransform;
                tr.localScale =
                    new Vector3(1 - value, tr.localScale.y, tr.localScale.z);

                // Icon
                var tr2 = carIcon.rectTransform;
                tr2.localPosition =
                    new Vector3(Mathf.Lerp(minX, maxX, value), tr2.localPosition.y, tr2.localPosition.z);
            }
        }
    }
}