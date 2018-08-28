using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ResFramework;
using System;
using Utility;
using Utility.SheetLite;
using UnityEngine.SceneManagement;
using Net;
using System.Text;
using System.IO;
using Google.Protobuf;
using System.Collections;
using UIFramework;
using Game;

namespace GameFramework
{
    public class Main : MonoBehaviour
    {
        public static Main Instance;

        public static event Action<float> EventUpdate;

        public static event Action EventLateUpdate;

        public static event Action EventFixedUpdate;

        public static event Action<bool> EventAppFocus;

        public static event Action<bool> EventAppPause;

        public static event Action EventAppQuit;

        [SerializeField]
        private eResLoadMode m_res_load_mode = eResLoadMode.Editor;

        public eResLoadMode ResLoadMode { get { return m_res_load_mode; } }

        [SerializeField]
        private bool m_check_update = false;

        public Image TestImage = null;
        public Image TestImage1 = null;
        public Text TestText = null;
        public Text TestText1 = null;

        private GameStateMachine m_game_machine;

        void Awake()
        {
            Instance = this;
            m_game_machine = new GameStateMachine();
            m_game_machine.Initialize();
            DontDestroyOnLoad( this );
        }

        void Start()
        {
            m_game_machine.Start();
            //LunarConsolePlugin.LunarConsole.Show();
            //下面是测试 正常应该放在游戏状态机里管理
            ResManager.Instance.Init( m_res_load_mode );
            Action action = () =>
            {
                //测试lua_proto
                List<string> res_list = new List<string>();
                res_list.AddRange( new string[] { "pbs.assetbundle", "luas.assetbundle" } );
                ResManager.Instance.LoadBundleList( res_list, () =>
                {
                    LuaManager.Instance.Init();
                    ////测试UI
                    UIManager.Instance.ShowUI( "ui_test_lua" );
                    StartCoroutine( _testEvent() );
                    //测试pbc
                    //Msg.LoginRequest msg = new Msg.LoginRequest();
                    //msg.Id = 1000;
                    //msg.Name = "zyp";
                    //msg.Email = "1@qq.com";
                    //msg.Sid = 8888;
                    //byte[] result;
                    //using( MemoryStream ms = new MemoryStream() )
                    //{
                    //    msg.WriteTo( ms );
                    //    result = ms.ToArray();
                    //}
                    //LuaManager.Instance.Call( "TestPbc", result );
                    ////LuaManager.Instance.Call( "TestPb", result );
                    //Debug.Log( Network.player.ipAddress );
                } );
                //UIFramework.UIManager.Instance.Initialize();
                //////测试shader
                //ResManager.Instance.LoadBundle( "shaders.assetbundle", (_data, _obj) =>
                //{
                //    if( _data != null )
                //    {
                //        _data.GetBundle().LoadAllAssets();
                //        _data.GetBundle().LoadAsset<ShaderVariantCollection>( "ShaderVariants" ).WarmUp();
                //        Debug.Log( "所有shader预热完成" );
                //    }
                //    ResManager.Instance.LoadAsset( "Assets/Res/TestShader/Cube.prefab", (__data, __obj) => { Instantiate( __obj ); if( __data != null ) __data.Unload(); } );
                //} );
                ////测试自定义csv
                //CsvConfig.LoadCsvConfig( "global_config", (_data) =>
                //{
                //    for( int i = 0; i < _data.Count; ++i )
                //    {
                //        SheetRow row = _data[i];
                //        string key = row["Key"];
                //        string value = row["Value"];
                //    }
                //} );
                //////测试反射csv
                //Dictionary<string, TestCsv> dic = new Dictionary<string, TestCsv>();
                //CsvConfig.LoadCsvConfigWithClassKey<string, TestCsv>( "global_config", dic );
                //Dictionary<int, TestCsv1> dic1 = new Dictionary<int, TestCsv1>();
                //CsvConfig.LoadCsvConfigWithStructKey<int, TestCsv1>( "global_config1", dic1 );
                //测试加载场景
                //ResManager.Instance.LoadScene( "Assets/Scene/test/test.unity", (_data, _obj) =>
                //{
                //    if( _data != null )
                //        SceneManager.LoadScene( "test" );
                //} );
                //测试音乐
                //StartCoroutine( _testMusic() );
                //测试pool
                //StartCoroutine( _testPool() );
                //测试UIEffect
                //ResManager.Instance.LoadBundle( "shaders.assetbundle", (_data, _obj) =>
                //{
                //    if( _data != null )
                //    {
                //        ShaderManager.Instance.AddShaders( _data.GetBundle().LoadAllAssets<Shader>() );
                //         _data.GetBundle().LoadAsset<ShaderVariantCollection>( "ShaderVariants" ).WarmUp();
                //        _data.Unload();
                //        Debug.Log( "所有shader预热完成" );
                //    }
                //    //UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( "Assets/Res/UI/Prefab/ui_test_c.prefab" );
                //    //GameObject go = Instantiate( obj ) as GameObject;
                //    //go.SetActive( true );
                //    UIManager.Instance.ShowUI( "ui_test_uieffect" );
                //    //TestImage.SetGrayEffect( true );
                //    //TestImage.SetBlurEffect( true, 0.5f );
                //    //TestImage.SetPixelEffect( true );
                //    //TestImage1.SetGrayEffect( true, 0.5f );
                //    //TestImage1.SetPixelEffect( true, 0.5f );
                //    //TestImage1.SetBlurEffect( true );

                //    //ui_test_uieffect
                //    //TestText.SetGrayEffect( true );
                //    //TestText.SetPixelEffect( true, 0.2f );
                //    //TestText.SetBlurEffect( true, 0.5f );
                //    //TestText1.SetGrayEffect( true, 0.5f );
                //    //TestText1.SetPixelEffect( true, 1f );
                //    //TestText1.SetBlurEffect( true, 0.6f );
                //} );
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
            NetManager.Instance.Update();
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
            NetManager.Instance.OnClose();
            if( EventAppQuit != null )
                EventAppQuit();
        }

        IEnumerator _testMusic()
        {
            SoundFramework.SoundManager.PlayMusic( "test_music.ogg" );
            yield return new WaitForSeconds( 5 );
            SoundFramework.SoundManager.PlayMusic( "test_music1.ogg", 1f, false, false, 3f );
            yield return new WaitForSeconds( 2 );
            SoundFramework.SoundManager.PlaySound( "test_sound.ogg" );
        }

        IEnumerator _testPool()
        {
            TrashManRecycleBin data = new TrashManRecycleBin();
            GameObject prefab = new GameObject();
            data.prefab = prefab;
            TrashMan.manageRecycleBin( data );
            for( int i = 0; i < 66; ++i )
            {
                var newObj = TrashMan.spawn( prefab );
                TrashMan.despawnAfterDelay( newObj, 2f );
                yield return new WaitForSeconds( 3f );
            }
        }

        IEnumerator _testEvent()
        {
            yield return new WaitForSeconds( 5 );
            EventSystem.Instance.OnEvent( eEvents.TestEvent, 1, 2, 3 );
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
