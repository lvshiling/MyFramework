using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    public static class UIEffect
    {
        public static void SetGrayEffect( this Image _image, bool _enabled )
        {
            Shader shader = GameFramework.ShaderManager.Instance.GetShader( "UI/UIEffect" );
            if( shader == null )
            {
                Debug.LogError( "没有shader:UI/UIEffect" );
                return;
            }
            if( shader != _image.material.shader )
            {
                _image.material = new Material( shader );
            }
            if( _enabled )
                _image.material.EnableKeyword( "GRAY_EFFECT" );
            else
                _image.material.DisableKeyword( "GRAY_EFFECT" );
        }
    }
}
