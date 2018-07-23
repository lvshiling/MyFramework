using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    public static class UIEffect
    {
        public static Material GetMaterial( string _shader, Graphic _graphic )
        {
            Shader shader = GameFramework.ShaderManager.Instance.GetShader( _shader );
            if( shader == null )
            {
                Debug.LogErrorFormat( "没有shader:{0}", _shader );
                return null;
            }
            if( shader != _graphic.material.shader )
            {
                _graphic.material = new Material( shader );
            }
            return _graphic.material;
        }

        public static void SetGrayEffect( this Graphic _graphic, bool _enabled, float _power = 1.0f )
        {
            Material material = GetMaterial( "UI/UIEffect", _graphic );
            if( material == null )
                return;
            if( _enabled )
            {
                material.EnableKeyword( "GRAY_EFFECT" );
                material.SetFloat( "_GrayPower", _power );
            }
            else
                material.DisableKeyword( "GRAY_EFFECT" );
        }

        public static void SetPixelEffect( this Graphic _graphic, bool _enabled, int _power = 12 )
        {
            Material material = GetMaterial( "UI/UIEffect", _graphic );
            if( material == null )
                return;
            if( _enabled )
            {
                material.EnableKeyword( "PIXEL_EFFECT" );
                material.SetFloat( "_PixelPower", _power );
            }
            else
                material.DisableKeyword( "PIXEL_EFFECT" );
        }
    }
}
