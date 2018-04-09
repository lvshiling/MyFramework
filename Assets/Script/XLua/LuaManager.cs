using XLua;
using ResFramework;
using UnityEngine;
using System;

namespace GameFramework
{
    public class LuaManager
    {
        public static LuaManager Instance = new LuaManager();

        private LuaEnv m_lua_env = null;

        public void Init()
        {
            m_lua_env = new LuaEnv();
            //m_lua_env.AddBuildin( "pb", XLua.LuaDLL.Lua.LoadPb );
            //m_lua_env.AddBuildin( "rapidjson", XLua.LuaDLL.Lua.LoadRapidJson );
            m_lua_env.AddBuildin( "protobuf.c", XLua.LuaDLL.Lua.LoadProtobufC );
            m_lua_env.AddLoader( (ref string path) =>
            {
#if UNITY_EDITOR
                if( ResManager.Instance.ResLoadMode == eResLoadMode.Editor )
                    return System.IO.File.ReadAllBytes( string.Format( "Assets/LuaScript/{0}.lua.txt", path ) );
#endif
                TextAsset asset = null;
                ResManager.Instance.LoadAsset( string.Format( "Assets/LuaScript/{0}.lua.txt", path ), (_res_data, _obj) =>
                {
                    asset = _obj as TextAsset;
                }, false );
                return asset.bytes;
            } );
            m_lua_env.DoString( "require 'Init'" );
        }

        public void UnInit()
        {
            m_lua_env.Dispose();
        }

        public void RequireLua( string _name, Action<object> _call_back )
        {
            _name = string.Format( "Assets/LuaScript/{0}.lua.txt", _name );
#if UNITY_EDITOR
            if( ResManager.Instance.ResLoadMode == eResLoadMode.Editor )
            {
                object[] datas = m_lua_env.DoString( System.IO.File.ReadAllBytes( _name ) );
                if( _call_back != null )
                {
                    _call_back( datas[0] );
                }
            }
#endif
            if( ResManager.Instance.ResLoadMode == eResLoadMode.Bundle )
            {
                ResManager.Instance.LoadAsset( _name, (_res_data, _obj) =>
                {
                    object[] datas = m_lua_env.DoString( ( _obj as TextAsset ).bytes );
                    if( _call_back != null )
                    {
                        _call_back( datas[0] );
                    }
                }, false );
            }
        }

        //      /// <summary>
        //      ///     取Lua层的全局变量
        //      /// </summary>
        //      /// <typeparam name="T">返回的类型</typeparam>
        //      /// <param name="name">变量名</param>
        //      /// <returns>返回该变量的值</returns>
        //      public static T Get<T>(string name) {
        //          if (lua == null) {
        //              Logger.Error("Lua.Initialize should be called before Lua.Get<T>");
        //              return default(T);
        //          }

        //          return lua.Global.Get<T>(name);
        //      }

        //      /// <summary>
        //      ///     取路径下的变量值，如math.max等
        //      /// </summary>
        //      /// <typeparam name="T">返回值类型</typeparam>
        //      /// <param name="path">变量的路径</param>
        //      /// <returns>返回该变量的值</returns>
        //      public static T GetInPath<T>(string path) {
        //          if (lua == null) {
        //		Logger.Error("Lua.Initialize should be called before Lua.GetInPath<T>");
        //              return default(T);
        //          }

        //          return lua.Global.GetInPath<T>(path);
        //      }

        ///// <summary>
        /////		创建LuaTable
        ///// </summary>
        ///// <returns>新创建的LuaTable</returns>
        //public static LuaTable NewTable() {
        //	return lua.NewTable();
        //}

        //      /// <summary>
        //      ///     调用Lua层函数（请仅用在用户输入事件或其他非频繁调用的地方）
        //      /// </summary>
        //      /// <param name="func">函数地址，可以为math.max等</param>
        //      /// <param name="args">调用参数</param>
        //      public static void Call(string func, params object[] args) {
        //          if (lua == null) {
        //		Logger.Error("Lua.Initialize should be called before Lua.Call");
        //              return;
        //          }

        //          LuaFunction f = lua.Global.GetInPath<LuaFunction>(func);
        //          if (f == null) {
        //		Logger.Error("Lua.Call error no function named : " + func);
        //              return;
        //          }

        //          f.Invoke(args);
        //      }

        //      /// <summary>
        //      ///     直接指定返回值的Call
        //      /// </summary>
        //      /// <typeparam name="T">返回值类型</typeparam>
        //      /// <param name="func">函数地址，可以为math.max等非全局函数</param>
        ///// <param name="args">参数列表</param>
        //      /// <returns>调用返回值</returns>
        //      public static T Call<T>(string func, params object[] args) {
        //          if (lua == null) {
        //		Logger.Error("Lua.Initialize should be called before Lua.Call<T>");
        //              return default(T);
        //          }

        //          LuaFunction f = lua.Global.GetInPath<LuaFunction>(func);
        //          if (f == null) {
        //		Logger.Error("Lua.Call<T> error no function named : " + func);
        //              return default(T);
        //          }

        //          return f.Invoke<T>(args);
        //      }
    }

}
