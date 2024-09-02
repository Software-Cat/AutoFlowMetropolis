using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Concurrent {
  public class GUI : MonoBehaviour {
    private Text emissionText;
    private Text tripTimeText;
    private Text idleTimeText;
    public Font inter; 

    private void Start() {
      // create the white canvas
      GameObject canvasGO = new GameObject("Canvas");
      Canvas canvas = canvasGO.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvasGO.AddComponent<CanvasScaler>();
      canvasGO.AddComponent<GraphicRaycaster>();

      // creat the panel with its colours, size and position
      GameObject panelGO = new GameObject("Panel");
      panelGO.transform.SetParent(canvasGO.transform, false);
      RectTransform panelRect = panelGO.AddComponent<RectTransform>();
      panelRect.sizeDelta = new Vector2(400, 250);
      panelRect.anchorMin = new Vector2(0, 1);
      panelRect.anchorMax = new Vector2(0, 1);
      panelRect.pivot = new Vector2(0, 1);
      panelRect.anchoredPosition = new Vector2(10, -10);
      panelGO.AddComponent<CanvasRenderer>();
      Image panelImage = panelGO.AddComponent<Image>();
      panelImage.color = new Color(255, 255, 255, 1f);

      // programatically create the text elements with displacements of 80 pixels from each other
      emissionText = CreateTextElement(panelGO.transform, new Vector2(10, -10), "Emissions: 0.00g");
      tripTimeText = CreateTextElement(panelGO.transform, new Vector2(10, -90), "Trip Time: 0.00s");
      idleTimeText = CreateTextElement(panelGO.transform, new Vector2(10, -170), "Idle Time: 0.00s");
    }

    private Text CreateTextElement(Transform parent, Vector2 position, string initialText) {
      // create the any element with the given position and text
      GameObject textGO = new GameObject("Text");
      textGO.transform.SetParent(parent, false);
      RectTransform rectTransform = textGO.AddComponent<RectTransform>();
      rectTransform.anchorMin = new Vector2(0, 1);
      rectTransform.anchorMax = new Vector2(0, 1);
      rectTransform.pivot = new Vector2(0, 1);
      rectTransform.anchoredPosition = position;
      rectTransform.sizeDelta = new Vector2(400, 80);

      // set its font, size, colour and text
      Text text = textGO.AddComponent<Text>();
      text.font = inter != null ? inter : Resources.GetBuiltinResource<Font>("Arial.ttf");
      text.fontSize = 28;
      text.color = Color.black;
      text.text = initialText;

      return text;
    }

    private void Update() {
      RoadGenerator rg = GameObject.Find("RoadGenerator").GetComponent<RoadGenerator>();
      if (rg == null) {
        return;
      }

      // get the vehicles and calculate the cumulative emissions, trip time and idle time
      List<Vehicle2D> vehicles = new List<Vehicle2D>();
      foreach (KeyValuePair<int, Vehicle2D> entry in rg.vehicles) {
        vehicles.Add(entry.Value);
      }
      float cumulEm = 0, cumulTrip = 0, cumulIdle = 0;
      foreach (Vehicle2D vehicle in vehicles) {
        cumulEm += vehicle.cumulativeEmission;
        cumulTrip += vehicle.tripTime;
        cumulIdle += vehicle.idleTime;
      }

      // update the text elements with their new values
      emissionText.text = $"Emissions: {cumulEm:F2} g";
      tripTimeText.text = $"Trip Time: {cumulTrip:F2}s";
      idleTimeText.text = $"Idle Time: {cumulIdle:F2}s";
    }
  }
}