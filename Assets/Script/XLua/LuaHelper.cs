using ResFramework;
using UnityEngine;

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
    }

}
