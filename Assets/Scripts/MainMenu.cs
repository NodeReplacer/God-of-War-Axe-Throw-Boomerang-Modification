using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        Cursor.visible = true;
    }
    public void StartSimple()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void StartAnimated()
    {
        SceneManager.LoadScene("Making animated dude scene");
    }
}
