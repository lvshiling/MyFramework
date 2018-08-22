using ResFramework;
using UnityEngine;
using Utility;

namespace Game
{
    public class LuaHelper
    {
        public static byte[] LoadPb( string _name )
        {
            _name = string.Format( "Assets/LuaScript/Pb/{0}.pb.bytes", _name );
            TextAsset asset = null;
            ResManager.Instance.LoadAsset( _name, ( _res_data, _obj )=>
            {
                asset = _obj as TextAsset;
            }, false );
            return asset.bytes;
        }

        public static void AddEvent( eEvents _event, GameEventHandler _handler )
        {
            EventSystem.Instance.AddEvent( _event, _handler );
        }

        public static void RemoveEvent( eEvents _event, GameEventHandler _handler )
        {
            EventSystem.Instance.RemoveEvent( _event, _handler );
        }
    }

}
