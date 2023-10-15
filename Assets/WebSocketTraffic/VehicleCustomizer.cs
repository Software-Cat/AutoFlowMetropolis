using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WebSocketTraffic
{
    public class VehicleCustomizer : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        public List<Material> materialChoices;
        public List<Material> ghostMaterial;

        private void Start()
        {
            var choice = materialChoices[Random.Range(0, materialChoices.Count)];
            var originals = meshRenderer.materials.ToList();
            originals[0] = choice;

            meshRenderer.SetMaterials(originals);
        }

        public void GhostMode()
        {
            meshRenderer.SetMaterials(ghostMaterial);
        }
    }
}
