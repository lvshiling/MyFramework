using System;

namespace Utility
{
	/// <summary>
	/// 字符串分隔类
	/// </summary>
	class TokenString
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="_value">要分隔的字符串</param>
		/// <param name="_delimiters">分隔符数组</param>
		public TokenString( String _value, Char[] _delimiters )
		{
			String[] split_array = _value.Split( _delimiters );
			m_tokens = new AnyString[split_array.Length];
			for( Int32 i = 0; i < split_array.Length; ++i )
			{
				m_tokens[i] = new AnyString( split_array[i] );
			}
		}
		/// <summary>
		/// 分隔后数据的数组索引访问
		/// </summary>
		/// <param name="_index">获取第几个数据，从0开始</param>
		/// <returns>返回指定索引数据，超出范围返回空串值（not null）</returns>
		public AnyString this[Int32 _index]
		{
			get
			{
				if( _index < 0 || _index >= m_tokens.Length )
				{
					return new AnyString();
				}
				return m_tokens[_index];
			}
		}
		public Int32 Length { get { return m_tokens.Length; } }
		private AnyString[] m_tokens = null;
	}
}
