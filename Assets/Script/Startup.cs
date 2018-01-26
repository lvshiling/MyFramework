using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ResFramework;


namespace GameFramework
{
    public class Startup : MonoBehaviour
    {
        [SerializeField]
        private eResLoadMode m_res_load_mode = eResLoadMode.Editor;
        // Use this for initialization
        void Start()
        {
            ResManager.Instance.Init( m_res_load_mode );
            LuaManager.Instance.Init();
            UIFrameWork.UIManager.Instance.Initialize();
            UIFrameWork.UIManager.Instance.ShowUI( "ui_test_lua" );
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
