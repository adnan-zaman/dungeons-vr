using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


/**
 * On click, start the game
 */ 
public class MenuItem : UIBehaviour
{
    public Text menuItem;
    private EventSystem eventSystem;

    protected override void Start()
    {
        eventSystem = EventSystem.current;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("Level1");
        }

    }
}

   
