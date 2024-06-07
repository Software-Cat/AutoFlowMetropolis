using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{



    // scene name
    public string sceneName;

    // // change scene on click
    // public void ChangeSceneOnClick()
    // {
    //     SceneManager.LoadScene("SimConfig");
    // }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("SimConfig");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
