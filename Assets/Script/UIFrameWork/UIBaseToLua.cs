using System;
using UnityEngine;
using UnityEngine.UI;
using XLua;
using GameFramework;

namespace UIFrameWork
{
    /// <summary>
    /// 全脚本逻辑UI用这个
    /// </summary>
    public class UIBaseToLua : UIBase
    {
        private LuaTable m_lua_self;

        public override void OnLoaded( object _obj )
        {
            m_lua_self = _obj as LuaTable;
            LuaFunction func = m_lua_self.GetInPath<LuaFunction>( "OnLoad" );
            if ( func != null )
                func.Call( gameObject );
        }

        public override void PreShow( UIBase _pre_ui, params object[] _args )
        {
            LuaFunction func = m_lua_self.GetInPath<LuaFunction>( "PreShow" );
            if ( func != null )
                func.Call( _args );
        }

        public override void OnShow( UIBase _pre_ui, params object[] _args )
        {
            LuaFunction func = m_lua_self.GetInPath<LuaFunction>( "OnShow" );
            if ( func != null )
                func.Call( _args );
        }

        public override void OnHide( UIBase _next_ui )
        {
            LuaFunction func = m_lua_self.GetInPath<LuaFunction>( "OnHide" );
            if (func != null)
                func.Call();
        }

        public override void OnUnload()
        {
            LuaFunction func = m_lua_self.GetInPath<LuaFunction>( "OnUnload" );
            if ( func != null )
                func.Call();
        }
    }
}