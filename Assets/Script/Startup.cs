﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ResFramework;

public class Startup : MonoBehaviour
{
    [SerializeField]
    private ResLoadMode m_res_load_mode = ResLoadMode.ResLoadMode_Editor;
	// Use this for initialization
	void Start ()
    {
        ResManager.Instance.Init( m_res_load_mode );
        UIFrameWork.UIManager.Instance.Initialize();
        UIFrameWork.UIManager.Instance.ShowUI( "ui_test_panel" );
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}