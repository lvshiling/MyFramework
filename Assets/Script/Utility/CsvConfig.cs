using System.Collections;
using System.Collections.Generic;
using System;
using Utility.SheetLite;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace Utility
{
    public class CsvConfig
    {
        public static void LoadCsvConfig( string _name, Action<SheetReader> _action )
        {
            ResFramework.ResManager.Instance.LoadAsset( string.Format( "Assets/Res/Config/{0}.txt", _name ), 
            ( _data, _object ) => 
            {
                Utility.SheetLite.SheetReader reader = new Utility.SheetLite.SheetReader();
                TextAsset text = _object as TextAsset;
                if( reader.OpenSheet( _name, text.bytes ) )
                    _action( reader );
                if( _data != null )
                    _data.Unload();
            }, false );
        }

        public static void LoadCsvConfigWithClassKey<Tkey, Tvalue>( string _name, Dictionary<Tkey, Tvalue> _dic ) where Tkey : class where Tvalue : new()
        {
            ResFramework.ResManager.Instance.LoadAsset( string.Format( "Assets/Res/Config/{0}.txt", _name ),
            ( _data, _object ) =>
            {
                Utility.SheetLite.SheetReader reader = new Utility.SheetLite.SheetReader();
                TextAsset text = _object as TextAsset;
                if( !reader.OpenSheet( _name, text.bytes ) )
                    return;
                for( int i = 0; i < reader.Count; ++i )
                {
                    SheetRow row = reader[i];
                    Type type = typeof( Tvalue );
                    Tkey new_key = ConverFieldValue( reader.Sheet.GetKeyTypeString( 0 ), row[0] ) as Tkey;
                    Tvalue new_value = new Tvalue();
                    for( int j = 0; j < row.Count; ++j )
                    {
                        if( string.IsNullOrEmpty( row[j] ) )
                            continue;
                        string key = reader.Sheet.GetKeyString( j );
                        string key_type = reader.Sheet.GetKeyTypeString( j );
                        try
                        {
                            FieldInfo Property = type.GetField( key );
                            Property.SetValue( new_value, ConverFieldValue( key_type, row[j] ) );
                        }
                        catch( Exception e )
                        {
                            Debug.LogError( string.Format( "表头 {0} 不是{1}类型 检查类 {2} 对应的配置 !!!{3}", key, key_type, type.Name, e.Message ) );
                        }
                    }
                    _dic.Add( new_key, new_value );
                }
                if( _data != null )
                    _data.Unload();
            }, false );
        }

        public static void LoadCsvConfigWithStructKey<Tkey, Tvalue>( string _name, Dictionary<Tkey, Tvalue> _dic ) where Tkey : struct where Tvalue : new()
        {
            ResFramework.ResManager.Instance.LoadAsset( string.Format( "Assets/Res/Config/{0}.txt", _name ),
            (_data, _object) =>
            {
                Utility.SheetLite.SheetReader reader = new Utility.SheetLite.SheetReader();
                TextAsset text = _object as TextAsset;
                if( !reader.OpenSheet( _name, text.bytes ) )
                    return;
                for( int i = 0; i < reader.Count; ++i )
                {
                    SheetRow row = reader[i];
                    Type type = typeof( Tvalue );
                    Tkey new_key = (Tkey)ConverFieldValue( reader.Sheet.GetKeyTypeString( 0 ), row[0] );
                    Tvalue new_value = new Tvalue();
                    for( int j = 0; j < row.Count; ++j )
                    {
                        if( string.IsNullOrEmpty( row[j] ) )
                            continue;
                        string key = reader.Sheet.GetKeyString( j );
                        string key_type = reader.Sheet.GetKeyTypeString( j );
                        try
                        {
                            FieldInfo Property = type.GetField( key );
                            Property.SetValue( new_value, ConverFieldValue( key_type, row[j] ) );
                        }
                        catch( Exception e )
                        {
                            Debug.LogError( string.Format( "表头 {0} 不是{1}类型 检查类 {2} 对应的配置 !!!{3}", key, key_type, type.Name, e.Message ) );
                        }
                    }
                    _dic.Add( new_key, new_value );
                }
                if( _data != null )
                    _data.Unload();
            }, false );
        }

        public static object ConverFieldValue( string _key_type, string _value )
        {
            object o = _value;
            try
            {
                switch( _key_type )
                {
                    case "byte":
                    o = Convert.ToByte( _value );
                    break;
                    case "short":
                    o = Convert.ToInt16( _value );
                    break;
                    case "int":
                    o = Convert.ToInt32( _value );
                    break;
                    case "int64":
                    o = Convert.ToInt64( _value );
                    break;
                    case "float":
                    o = Convert.ToSingle( _value );
                    break;
                    case "bool":
                    o = Convert.ToBoolean( _value );
                    break;
                    case "string":
                    break;
                }
            }
            catch( Exception ex )
            {
                Debug.Log( "ex = " + ex.Message );
            }
            return o;
        }
    }
}
