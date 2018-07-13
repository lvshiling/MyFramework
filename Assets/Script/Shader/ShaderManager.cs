using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ResFramework;

namespace GameFramework
{
    public class ShaderManager
    {
        public static ShaderManager Instance = new ShaderManager();

        private Dictionary<string, Shader> m_shaders = new Dictionary<string, Shader>();

        public void InitShader()
        {
            m_shaders.Clear();
            ResManager.Instance.LoadBundle( "shaders.assetbundle", ( _data, _obj ) =>
            {
                if( _data != null )
                {
                    AddShaders( _data.GetBundle().LoadAllAssets<Shader>() );
                    _data.GetBundle().LoadAsset<ShaderVariantCollection>( "ShaderVariants" ).WarmUp();
                    _data.Unload();
                    Debug.Log( "所有shader预热完成" );
                }
            } );
        }

        public void AddShaders( Shader[] _values )
        {
            for( int i = 0; i < _values.Length; ++i )
            {
                AddShader( _values[i] );
            }
        }

        public void AddShader( Shader _shader )
        {
            if( m_shaders.ContainsKey( _shader.name ) )
                return;
            m_shaders.Add( _shader.name, _shader );
        }

        public Shader GetShader( string _name )
        {
            if( GameFramework.Main.Instance.ResLoadMode == ResFramework.eResLoadMode.Editor )
                return Shader.Find( _name );
            if( m_shaders.ContainsKey( _name ) )
                return m_shaders[_name];
            return null;
        }
    }
}
