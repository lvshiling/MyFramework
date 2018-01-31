using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ResFramework;
using System;
using UnityEngine.Networking;

namespace GameFramework
{
    public class Startup : MonoBehaviour
    {
        [SerializeField]
        private eResLoadMode m_res_load_mode = eResLoadMode.Editor;

        [SerializeField]
        private bool m_check_update = false;

        void Awake()
        {
            DontDestroyOnLoad( this );
        }

        void Start()
        {
            ResManager.Instance.Init( m_res_load_mode );
            Action action = () =>
            {
                LuaManager.Instance.Init();
                UIFrameWork.UIManager.Instance.Initialize();
                UIFrameWork.UIManager.Instance.ShowUI( "ui_test_lua" );
            };
            if( m_check_update )
            {
                ResUpdate.Instance.CheckUpdate( action );
            }
            else
            {
                action();
            }
        }

        void Update()
        {

        }
    }
}
