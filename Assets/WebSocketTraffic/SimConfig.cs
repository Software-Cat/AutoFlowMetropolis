using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace WebSocketTraffic {
    public class SimConfig : MonoBehaviour {
        private string[] options = new string[] {"Sydney", "Melbourne", "Manhattan", "Los Angeles", "London", "Tokyo"};
        public int selectedIndex = -1;
        public Font font;

        public float vehicleDensity = 0f;
        public float autoFlowPercent = 0f;
        public float mapSize = 0f;

        public bool fullDay = false;
        public bool receiveNewDests = false;
        public bool graphics = false;
        public bool roadBlockage = false;

        public bool useRealLandscape = false;

        void OnDisable()
        {   
            // save the values to playerprefs so it can be accessed in the next scene
            PlayerPrefs.SetFloat("vehicleDensity", vehicleDensity);
            PlayerPrefs.SetFloat("autoFlowPercent", autoFlowPercent);
            PlayerPrefs.SetFloat("mapSize", mapSize);
            PlayerPrefs.SetInt("selectedIndex", selectedIndex);
            PlayerPrefs.SetInt("fullDay", fullDay ? 1 : 0);
            PlayerPrefs.SetInt("receiveNewDests", receiveNewDests ? 1 : 0);
            PlayerPrefs.SetInt("graphics", graphics ? 1 : 0);
            PlayerPrefs.SetInt("roadBlockage", roadBlockage ? 1 : 0);
        }

        // helper method to create a texture with a border (not supported natively)
        Texture2D CreateTextureWithBorder(float w, float h, Color borderColor, int borderThickness)
        {
            int width = (int)w;
            int height = (int)h;
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
            
            // set the text color with transparency (RGBA)
            gridStyle.normal.textColor = new Color(0f, 0f, 0f, 1f); // white with 50% transparency
            // #296EDF
            gridStyle.hover.textColor = new Color(0.16f, 0.43f, 0.87f, 1f); 
            gridStyle.active.textColor = new Color(0.16f, 0.43f, 0.87f, 1f);
            gridStyle.padding = new RectOffset(40, 10, 10, 10);

            // create a texture and set it as the background for the button to change its color
            Texture2D buttonTexture = new Texture2D(1, 1);
            buttonTexture.SetPixel(0, 0, new Color(0.945f, 0.945f, 0.945f, 1f));
            buttonTexture.Apply();

            // keep aspect ratio constant, so we can scale the UI elements based on the screen size
            float horScale = Screen.width / 1920f;
            float verScale = Screen.height / 1080f;

            gridStyle.normal.background = buttonTexture;
            gridStyle.hover.background = buttonTexture; 
            gridStyle.active.background = buttonTexture; 

            // prepare the dimensions for the buttons
            float optionWidth = 565 * horScale;
            float optionHeight = 100 * verScale;
            float spaceBetweenOptions = 15 * verScale;

            for (int i = 0; i < options.Length; i++)
            {
                float buttonY = 250 * verScale + i * (optionHeight + spaceBetweenOptions);
                Rect buttonRect = new Rect(270 * horScale, buttonY, optionWidth, optionHeight);

                // check if the current button is selected
                if (selectedIndex == i)
                {
                    // create a texture with a border for the selected button
                    Texture2D selectedTexture = CreateTextureWithBorder(optionWidth, optionHeight, new Color(0.16f, 0.43f, 0.87f, 1f), 4);
                    gridStyle.normal.background = selectedTexture;
                }
                else
                {
                    // apply the original texture without border for unselected buttons
                    gridStyle.normal.background = buttonTexture;
                }

                if (GUI.Button(buttonRect, options[i], gridStyle))
                {
                    selectedIndex = i;
                    Debug.Log("Selected Index: " + selectedIndex);
                }
            }

            // example starting value
            float sliderMinValue = 0f;
            float sliderMaxValue = 100f;

            // create the Rect for the slider
            
            Rect sliderRect = new Rect(1100*horScale, 300*verScale, 400*horScale, 20*verScale); 

            // before drawing the slider, set the colors
            Color sliderBackgroundColor = new Color(0.93f, 0.93f, 0.93f, 1f);
            Color sliderThumbColor = new Color(0.16f, 0.43f, 0.87f, 1f); 

            // apply new color to the slider background
            GUI.skin.horizontalSlider.normal.background = CreateTextureWithColor(1, 1, sliderBackgroundColor);

            // apply new color to the slider thumb
            GUI.skin.horizontalSliderThumb.normal.background = CreateTextureWithColor(1, 1, sliderThumbColor);
            GUI.skin.horizontalSliderThumb.hover.background = CreateTextureWithColor(1, 1, sliderThumbColor);
            GUI.skin.horizontalSliderThumb.active.background = CreateTextureWithColor(1, 1, sliderThumbColor);

            // draw the slider and update vehicleDensity with the current value of the slider
            vehicleDensity = GUI.HorizontalSlider(sliderRect, vehicleDensity, sliderMinValue, sliderMaxValue);

            // display the current value of the slider
            GUIStyle labelStyle1 = new GUIStyle(GUI.skin.label);
            labelStyle1.font = font;
            labelStyle1.fontSize = 15;
            labelStyle1.normal.textColor = new Color(0f, 0f, 0f, 1f); 
            labelStyle1.hover.textColor = new Color(0f, 0f, 0f, 1f); 
            GUI.Label(new Rect(1550*horScale, 290*verScale, 100*horScale, 40*verScale), vehicleDensity.ToString("F0") + "%", labelStyle1);

            // create the next sliders manually
            Rect sliderRect2 = new Rect(1100*horScale, 400*verScale, 400*horScale, 20*verScale);
            autoFlowPercent = GUI.HorizontalSlider(sliderRect2, autoFlowPercent, sliderMinValue, sliderMaxValue);
            GUI.Label(new Rect(1550*horScale, 390*verScale, 100*horScale, 40*verScale), autoFlowPercent.ToString("F0") + "%", labelStyle1);

            Rect sliderRect3 = new Rect(1100*horScale, 490*verScale, 400*horScale, 20*verScale);
            mapSize = GUI.HorizontalSlider(sliderRect3, mapSize, sliderMinValue, sliderMaxValue);
            GUI.Label(new Rect(1550*horScale, 480*verScale, 100*horScale, 40*verScale), mapSize.ToString("F0") + "%", labelStyle1);

            // create all the toggle buttons
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.font = font;
            toggleStyle.fontSize = 20;
            toggleStyle.normal.textColor = new Color(0f, 0f, 0f, 1f);
            toggleStyle.hover.textColor = new Color(0f, 0f, 0f, 1f);
            toggleStyle.padding = new RectOffset(40, 10, 10, 10);
            toggleStyle.active.textColor = new Color(0.16f, 0.43f, 0.87f, 1f);
            Rect toggleRect = new Rect(1100*horScale, 580*verScale, 60*horScale, 60*verScale);
            fullDay = GUI.Toggle(toggleRect, fullDay, "", toggleStyle);
            Rect toggleRect2 = new Rect(1100*horScale, 680*verScale, 60*horScale, 60*verScale);
            graphics = GUI.Toggle(toggleRect2, graphics, "", toggleStyle);
            Rect toggleRect3 = new Rect(1260*horScale, 580*verScale, 60*horScale, 60*verScale);
            receiveNewDests = GUI.Toggle(toggleRect3, receiveNewDests, "", toggleStyle);
            Rect toggleRect4 = new Rect(1260*horScale, 680*verScale, 60*horScale, 60*verScale);
            roadBlockage = GUI.Toggle(toggleRect4, roadBlockage, "", toggleStyle);

            // buttons
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            Rect buttonRect2 = new Rect(1370*horScale, 800*verScale, 270*horScale, 200*verScale);
            Texture2D buttonTexture2 = new Texture2D(1, 1);
            buttonTexture2.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
            buttonTexture2.Apply();
            buttonStyle.normal.background = buttonTexture2;
            buttonStyle.hover.background = buttonTexture2;
            buttonStyle.active.background = buttonTexture2;

            // next button switches the scene to the simulation
            if (GUI.Button(buttonRect2, "", buttonStyle))
            {   
                if (graphics)
                {
                    SceneManager.LoadScene("WebCity");
                } else {
                    SceneManager.LoadScene("2D");
                }
                
            }

            // back button switches the scene to the landing page
            Rect buttonRect3 = new Rect(1050*horScale, 800*verScale, 270*horScale, 200*verScale);
            if (GUI.Button(buttonRect3, "", buttonStyle))
            {
                SceneManager.LoadScene("Landing");
            }

        }

        // helper method to create a texture with a specific color
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