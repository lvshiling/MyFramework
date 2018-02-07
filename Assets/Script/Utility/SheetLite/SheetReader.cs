using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.SheetLite
{
	public class SheetReader
	{
		public Boolean OpenSheet( String _sheet_name, byte[] _data )
		{
            try
            {
                Sheet = new SheetLite();
                return Sheet.Open( _sheet_name, _data );
            }
            catch( Exception e )
            {
                Debug.LogErrorFormat( "打开SheetLite文件 {0} 失败! \n 错误信息:  {1}.", _sheet_name, e.Message );
            }
            return false;
        }



		public SheetRow this[Int32 _index] { get { return Sheet[_index]; } }
		public Int32 Count { get { return Sheet.Count; } }

        public SheetLite Sheet { get; private set; }
    }
}