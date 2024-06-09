using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace WebSocketTraffic {
    public class SimConfig : MonoBehaviour {
        private string[] options = new string[] {"Sydney", "Melbourne", "Manhattan", "Los Angeles", "London", "Tokyo"};
        public int selectedIndex = 0;
        public Font font;

        public float vehicleDensity = 0f;
        public float autoFlowPercent = 0f;
        public float mapSize = 0f;

        public bool fullDay = false;
        public bool receiveNewDests = false;
        public bool graphics = false;
        public bool procedural = false;

        // Start is called before the first frame update
        void Start()
        {
            // Initialize your menu options here if needed
        }

        // Update is called once per frame
        void Update()
        {
            // Handle any updates here
        }

        void OnDisable()
        {
            PlayerPrefs.SetFloat("vehicleDensity", vehicleDensity);
            PlayerPrefs.SetFloat("autoFlowPercent", autoFlowPercent);
            PlayerPrefs.SetFloat("mapSize", mapSize);
            PlayerPrefs.SetInt("selectedIndex", selectedIndex);
            PlayerPrefs.SetInt("fullDay", fullDay ? 1 : 0);
            PlayerPrefs.SetInt("receiveNewDests", receiveNewDests ? 1 : 0);
            PlayerPrefs.SetInt("graphics", graphics ? 1 : 0);
            PlayerPrefs.SetInt("procedural", procedural ? 1 : 0);
        }

        Texture2D CreateTextureWithBorder(int width, int height, Color borderColor, int borderThickness)
        {
            Texture2D texture = new Texture2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Apply border color on the edges
                    if (x < borderThickness || y < borderThickness || x >= width - borderThickness || y >= height - borderThickness)
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        // Fill the center with the original button color
                        texture.SetPixel(x, y, new Color(0.945f, 0.945f, 0.945f, 1f));
                    }
                }
            }
            texture.Apply();
            return texture;
        }

        void OnGUI()
        {
            GUIStyle gridStyle = new GUIStyle(GUI.skin.button);
            gridStyle.fontSize = 30;
            gridStyle.font = font;
            gridStyle.alignment = TextAnchor.MiddleLeft;
            // Set the text color with transparency (RGBA)
            gridStyle.normal.textColor = new Color(0f, 0f, 0f, 1f); // White with 50% transparency
            // #296EDF
            gridStyle.hover.textColor = new Color(0.16f, 0.43f, 0.87f, 1f); 
            gridStyle.active.textColor = new Color(0.16f, 0.43f, 0.87f, 1f);
            gridStyle.padding = new RectOffset(40, 10, 10, 10);

            // Create a texture and set it as the background for the button to change its color
            Texture2D buttonTexture = new Texture2D(1, 1);
            buttonTexture.SetPixel(0, 0, new Color(0.945f, 0.945f, 0.945f, 1f));
            buttonTexture.Apply();

            gridStyle.normal.background = buttonTexture;
            gridStyle.hover.background = buttonTexture; // Use the same or different texture for hover
            gridStyle.active.background = buttonTexture; // Use the same or different texture for active

            int optionWidth = 565;
            int optionHeight = 100;
            int spaceBetweenOptions = 15;

            for (int i = 0; i < options.Length; i++)
            {
                float buttonY = 250 + i * (optionHeight + spaceBetweenOptions);
                Rect buttonRect = new Rect(270, buttonY, optionWidth, optionHeight);

                // Check if the current button is selected
                if (selectedIndex == i)
                {
                    // Create a texture with a border for the selected button
                    Texture2D selectedTexture = CreateTextureWithBorder(optionWidth, optionHeight, new Color(0.16f, 0.43f, 0.87f, 1f), 4); // Example with a red border
                    gridStyle.normal.background = selectedTexture;
                }
                else
                {
                    // Apply the original texture without border for unselected buttons
                    gridStyle.normal.background = buttonTexture;
                }

                if (GUI.Button(buttonRect, options[i], gridStyle))
                {
                    selectedIndex = i;
                    Debug.Log("Selected Index: " + selectedIndex);
                }
            }

            // Example starting value
            float sliderMinValue = 0f; // Minimum value of the slider
            float sliderMaxValue = 100f; // Maximum value of the slider

            // Create the Rect for the slider
            Rect sliderRect = new Rect(1100, 300, 400, 20); 

            // Before drawing the slider, set the colors
            Color sliderBackgroundColor = new Color(0.93f, 0.93f, 0.93f, 1f);
            Color sliderThumbColor = new Color(0.16f, 0.43f, 0.87f, 1f); 

            // Apply new color to the slider background
            GUI.skin.horizontalSlider.normal.background = CreateTextureWithColor(1, 1, sliderBackgroundColor);

            // Apply new color to the slider thumb
            GUI.skin.horizontalSliderThumb.normal.background = CreateTextureWithColor(1, 1, sliderThumbColor);
            GUI.skin.horizontalSliderThumb.hover.background = CreateTextureWithColor(1, 1, sliderThumbColor);
            GUI.skin.horizontalSliderThumb.active.background = CreateTextureWithColor(1, 1, sliderThumbColor);

            // Draw the slider and update vehicleDensity with the current value of the slider
            vehicleDensity = GUI.HorizontalSlider(sliderRect, vehicleDensity, sliderMinValue, sliderMaxValue);

            // Display the current value of the slider
            GUIStyle labelStyle1 = new GUIStyle(GUI.skin.label);
            labelStyle1.font = font;
            labelStyle1.fontSize = 15;
            labelStyle1.normal.textColor = new Color(0f, 0f, 0f, 1f); 
            labelStyle1.hover.textColor = new Color(0f, 0f, 0f, 1f); 
            GUI.Label(new Rect(1550, 290, 100, 40), vehicleDensity.ToString("F0") + "%", labelStyle1);


            Rect sliderRect2 = new Rect(1100, 400, 400, 20);
            autoFlowPercent = GUI.HorizontalSlider(sliderRect2, autoFlowPercent, sliderMinValue, sliderMaxValue);
            GUI.Label(new Rect(1550, 390, 100, 40), autoFlowPercent.ToString("F0") + "%", labelStyle1);

            Rect sliderRect3 = new Rect(1100, 490, 400, 20);
            mapSize = GUI.HorizontalSlider(sliderRect3, mapSize, sliderMinValue, sliderMaxValue);
            GUI.Label(new Rect(1550, 480, 100, 40), mapSize.ToString("F0") + "%", labelStyle1);

            // toggle
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.font = font;
            toggleStyle.fontSize = 20;
            toggleStyle.normal.textColor = new Color(0f, 0f, 0f, 1f);
            toggleStyle.hover.textColor = new Color(0f, 0f, 0f, 1f);
            toggleStyle.padding = new RectOffset(40, 10, 10, 10);
            toggleStyle.active.textColor = new Color(0.16f, 0.43f, 0.87f, 1f);
            Rect toggleRect = new Rect(1100, 580, 60, 60);
            fullDay = GUI.Toggle(toggleRect, fullDay, "", toggleStyle);
            Rect toggleRect2 = new Rect(1100, 680, 60, 60);
            graphics = GUI.Toggle(toggleRect2, graphics, "", toggleStyle);
            Rect toggleRect3 = new Rect(1260, 580, 60, 60);
            receiveNewDests = GUI.Toggle(toggleRect3, receiveNewDests, "", toggleStyle);
            Rect toggleRect4 = new Rect(1260, 680, 60, 60);
            procedural = GUI.Toggle(toggleRect4, procedural, "", toggleStyle);

            // button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            Rect buttonRect2 = new Rect(1370, 800, 270, 200);
            Texture2D buttonTexture2 = new Texture2D(1, 1);
            buttonTexture2.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
            buttonTexture2.Apply();
            buttonStyle.normal.background = buttonTexture2;
            buttonStyle.hover.background = buttonTexture2;
            buttonStyle.active.background = buttonTexture2;
            if (GUI.Button(buttonRect2, "", buttonStyle))
            {
                SceneManager.LoadScene("WebCity");
            }

            Rect buttonRect3 = new Rect(1050, 800, 270, 200);
            if (GUI.Button(buttonRect3, "", buttonStyle))
            {
                SceneManager.LoadScene("Landing");
            }

        }

        Texture2D CreateTextureWithColor(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            return texture;
        }
    }
}