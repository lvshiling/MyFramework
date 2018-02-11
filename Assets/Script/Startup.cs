﻿using System.Collections.Generic;
using UnityEngine;
using ResFramework;
using System;
using Utility;
using Utility.SheetLite;

namespace GameFramework
{
    public class Startup : MonoBehaviour
    {
        public static event Action<float> EventUpdate;

        public static event Action EventLateUpdate;

        public static event Action EventFixedUpdate;

        public static event Action<bool> EventAppFocus;

        public static event Action<bool> EventAppPause;

        public static event Action EventAppQuit;

        [SerializeField]
        private eResLoadMode m_res_load_mode = eResLoadMode.Editor;

        [SerializeField]
        private bool m_check_update = false;

        private GameStateMachine m_game_machine;

        void Awake()
        {
            m_game_machine = new GameStateMachine();
            m_game_machine.Initialize();
            DontDestroyOnLoad( this );
        }

        void Start()
        {
            m_game_machine.Start();

            //下面是测试 正常应该放在游戏状态机里管理
            ResManager.Instance.Init( m_res_load_mode );
            Action action = () =>
            {
                LuaManager.Instance.Init();
                UIFrameWork.UIManager.Instance.Initialize();
                //测试UI
                UIFrameWork.UIManager.Instance.ShowUI( "ui_test_lua" );
                //测试shader
                ResManager.Instance.LoadAsset( "Assets/Res/TestShader/Cube.prefab", (_data, _obj) => { Instantiate( _obj ); _data.Unload(); ResManager.Instance.LoadAsset( "Assets/Res/TestShader/Cube.prefab", (__data, __obj) => { Instantiate( __obj ); __data.Unload(); } ); } );
                //测试自定义csv
                CsvConfig.LoadCsvConfig( "global_config", (_data) =>
                {
                    for( int i = 0; i < _data.Count; ++i )
                    {
                        SheetRow row = _data[i];
                        string key = row["Key"];
                        string value = row["Value"];
                    }
                } );
                //测试反射csv
                Dictionary<string, TestCsv> dic = new Dictionary<string, TestCsv>();
                CsvConfig.LoadCsvConfigWithClassKey<string, TestCsv>( "global_config", dic );
                Dictionary<int, TestCsv1> dic1 = new Dictionary<int, TestCsv1>();
                CsvConfig.LoadCsvConfigWithStructKey<int, TestCsv1>( "global_config1", dic1 );
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
            m_game_machine.Update();
            if( EventUpdate != null )
                EventUpdate( Time.deltaTime );
        }

        void FixedUpdate()
        {
            if( EventFixedUpdate != null )
                EventFixedUpdate();
        }

        void LateUpdate()
        {
            if( EventLateUpdate != null )
                EventLateUpdate();
        }

        void OnApplicationFocus( bool _focus )
        {
            if( EventAppFocus != null )
                EventAppFocus( _focus );
        }

        void OnApplicationPause( bool _pause )
        {
            if( EventAppPause != null )
                EventAppPause( _pause );
        }

        void OnApplicationQuit()
        {
            if( EventAppQuit != null )
                EventAppQuit();
        }

        class TestCsv
        {
            public string Key;
            public string Value;
            public int VariantType;
            public bool Description;
        }

        class TestCsv1
        {
            public int Key;
            public string Value;
            public int VariantType;
            public bool Description;
        }
    }
}
