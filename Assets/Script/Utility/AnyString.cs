using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility
{
	public class AnyString
	{
		public AnyString()
		{
		}
		public AnyString( String _value )
		{
			m_value = _value;
		}

		public static implicit operator AnyString( SByte _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}

		public static implicit operator AnyString( Byte _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}

		public static implicit operator AnyString( Int16 _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}
		public static implicit operator AnyString( UInt16 _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}
		public static implicit operator AnyString( Int32 _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}
		public static implicit operator AnyString( UInt32 _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}
		public static implicit operator AnyString( Int64 _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}
		public static implicit operator AnyString( UInt64 _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}
		public static implicit operator AnyString( float _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}
		public static implicit operator AnyString( double _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}
		public static implicit operator AnyString( String _v )
		{
			AnyString any = new AnyString( _v );
			return any;
		}
		public static implicit operator AnyString( Boolean _v )
		{
			AnyString any = new AnyString( _v.ToString() );
			return any;
		}

		// Operators
		public static implicit operator Byte( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "UInt8" );
			return Convert.ToByte( _v.m_value );
		}

		public static implicit operator SByte( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "Int8" );
			return Convert.ToSByte( _v.m_value );
		}

		public static implicit operator Int16( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "Int16" );
			return Convert.ToInt16( _v.m_value );
		}

		public static implicit operator UInt16( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "UInt16" );
			return Convert.ToUInt16( _v.m_value );
		}
		public static implicit operator Int32( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "Int32" );
			return Convert.ToInt32( _v.m_value );
		}
		public static implicit operator UInt32( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "UInt32" );
			return Convert.ToUInt32( _v.m_value );
		}
		public static implicit operator Int64( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "Int64" );
			return Convert.ToInt64( _v.m_value );
		}
		public static implicit operator UInt64( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "UInt64" );
			return Convert.ToUInt64( _v.m_value );
		}
		public static implicit operator Boolean( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "Boolean" );
			return Convert.ToBoolean( _v.m_value );
		}
		public static implicit operator float( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "float" );
			return Convert.ToSingle( _v.m_value );
		}
		public static implicit operator double( AnyString _v )
		{
			if( String.IsNullOrEmpty( _v.m_value ) )
				throw new Exception( "double" );
			return Convert.ToDouble( _v.m_value );
		}
		public static implicit operator String( AnyString _v )
		{
			return _v.m_value;
		}
		public static implicit operator Char( AnyString _v )
		{
			return _v.m_value[0];
		}
		public AnyString this[Int32 _index]
		{
			get
			{
				if( null == m_token )
				{
					m_token = new TokenString( m_value, s_delimiters );
				}
				return m_token[_index];
			}
		}

		public override String ToString()
		{
			return m_value;
		}

		private String m_value = String.Empty;
		private TokenString m_token = null;
		public static AnyString Empty = new AnyString();
		#region 字符串默认分隔
		public static void SetDelimiters( Char[] _delimiters )
		{
			s_delimiters = _delimiters;
		}
		private static Char[] s_delimiters = new Char[] { ',' };
		#endregion

	}
}
