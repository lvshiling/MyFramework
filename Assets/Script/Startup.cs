using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ResFramework;
using System;
using UnityEngine.Networking;
using Utility;
using Utility.SheetLite;

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
                //测试UI
                UIFrameWork.UIManager.Instance.ShowUI( "ui_test_lua" );
                //测试shader
                ResManager.Instance.LoadAsset( "Assets/Res/TestShader/Cube.prefab", ( _data, _obj )=> { Instantiate( _obj ); }, false );
                //测试自定义csv
                CsvConfig.LoadCsvConfig( "global_config", ( _data )=> 
                {
                    for( int i= 0; i < _data.Count; ++i )
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
