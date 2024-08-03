using System.Collections.Generic;
using UnityEngine;

namespace WebSocketTraffic
{
    public class TrafficLights : MonoBehaviour
    {
        public enum LightState
        {
            RED,
            YELLOW,
            GREEN,
            HIDDEN
        }

        public GameObject redLight;
        public GameObject yellowLight;
        public GameObject greenLight;

        public Material redMat;
        public Material yellowMat;
        public Material greenMat;
        public Material offMat;

        private LightState _state = LightState.RED;

        // visually depicting the various traffic light states by enabling/disabling the lights and changing their materials
        public LightState State
        {
            get => _state;
            set
            {
                if (value == _state) return;

                _state = value;
                switch (_state)
                {
                    case LightState.RED:
                        redLight.SetActive(true);
                        yellowLight.SetActive(true);
                        greenLight.SetActive(true);
                        redLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { redMat });
                        yellowLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { offMat });
                        greenLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { offMat });
                        break;
                    case LightState.YELLOW:
                        redLight.SetActive(true);
                        yellowLight.SetActive(true);
                        greenLight.SetActive(true);
                        redLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { offMat });
                        yellowLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { yellowMat });
                        greenLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { offMat });
                        break;
                    case LightState.GREEN:
                        redLight.SetActive(true);
                        yellowLight.SetActive(true);
                        greenLight.SetActive(true);
                        redLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { offMat });
                        yellowLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { offMat });
                        greenLight.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { greenMat });
                        break;
                    case LightState.HIDDEN:
                        redLight.SetActive(false);
                        yellowLight.SetActive(false);
                        greenLight.SetActive(false);
                        break;
                }
            }
        }

        private void Start()
        {
            State = LightState.HIDDEN;
        }
    }
}