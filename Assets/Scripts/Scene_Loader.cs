﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Scene_Loader : NetworkBehaviour  {

    public int ready_count = 0;
    //public List<Vector2> futureExits;
    public Vector3 nextRespawn;
    private Transform tp;
    private GameObject rp;
    //public int currentLevel;
	void Start () {
        tp = this.transform.GetChild(0);
		rp =  tp.GetComponent<TeleporterBox>().ObjectLocationToTPTo;
	}


    // Update is called once per frame
    void Update () {
    
       if (ready_count >=2)
        {

            rp.transform.position = nextRespawn;
            GetComponent<Exit_Door>().tp_count ++;
            tp.gameObject.SetActive(true);
            
            if (GetComponent<Exit_Door>().tp_count >=2)
            {
                this.enabled = false;
            }
   
        }

    }

    [ServerCallback]
    public void LoadOnline(string sceneName)
    {

        NetworkLobbyManager.singleton.ServerChangeScene(sceneName);
       
 
    }
}
