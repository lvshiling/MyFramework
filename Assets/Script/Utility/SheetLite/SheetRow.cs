using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.SheetLite
{
	public class SheetRow
	{
		public SheetRow( SheetLite _sheet )
		{
			m_sheet = _sheet;
		}

		public Int32 Count
		{
			get { return m_columns.Count; }
		}

		public void Append( String _value )
		{
			m_columns.Add( new AnyString( _value ) );
		}

		public AnyString this[Int32 _index]
		{
			get { return GetColumn( _index ); }
		}
		public AnyString this[String _index]
		{
			get
			{
				Int32 index = m_sheet.GetKeyIndex( _index );
				AnyString re = GetColumn( index );
				if( re == null )
					throw new Exception( string.Format( "SheetLiteRow: 尝试获取的Column: {0} 并不存在! sheet: {1}", _index, m_sheet.SheetName ) );
				return re;
			}
		}

		public Int32 GetKeyIndex( String _index )
		{
			return m_sheet.GetKeyIndex( _index );
		}

		private AnyString GetColumn( Int32 _index )
		{
			if( _index < 0 || _index >= m_columns.Count )
				return null;
			return m_columns[_index];
		}

		private List<AnyString> m_columns = new List<AnyString>();
		private SheetLite m_sheet = null;
	}
}
