#if !JXSJ_PUBLISH
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;


namespace RemoteDebugger {
    public delegate bool CustomCmdDelegate(string[] args);

    public class CustomCmdExecutor {
        private static CustomCmdExecutor _inst = null;
        private NetServer net_server = null;

        public static CustomCmdExecutor Instance {
            get {
                if (_inst == null) {
                    _inst = new CustomCmdExecutor();
                }
                return _inst;
            }
        }

        public NetServer net_conn {
            get { return net_server; }
            set { net_server = value; }
        }

        public void Init(NetServer net_server) {
            foreach (var method in typeof(CustomCmd).GetMethods(BindingFlags.Public |
                                                                BindingFlags.NonPublic |
                                                                BindingFlags.Instance)) {

                foreach (var attr in method.GetCustomAttributes(typeof(CustomCmdHandler), false)) {
                    try {
                        CustomCmdDelegate del = Delegate.CreateDelegate(typeof(CustomCmdDelegate), CustomCmd.Instance, method) as CustomCmdDelegate;
                        if (del != null) {
                            string szCmd = (attr as CustomCmdHandler).Command;
                            m_handlers[szCmd] = del;
                        }
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }

                }
            }

            if (net_server != null) {
                this.net_server = net_server;
            }
        }

        public void UnInit() {
            m_handlers.Clear();
        }

        public bool Execute(string[] arrayCmd) {
            CustomCmdDelegate _handler = null;
            if (m_handlers.TryGetValue(arrayCmd[0], out _handler)) {
                return _handler(arrayCmd);
            }
            else {
                return false;
            }
        }

        public Dictionary<string, CustomCmdDelegate> m_handlers = new Dictionary<string, CustomCmdDelegate>();
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomCmdHandler : Attribute {
        public CustomCmdHandler(string cmd) {
            Command = cmd;
        }

        public string Command;
    }

    public class CustomCmd {
        private static CustomCmd _inst = null;
        private CustomCmd() {

        }

        public static CustomCmd Instance {
            get {
                if (_inst == null) {
                    _inst = new CustomCmd();
                }
                return _inst;
            }
        }

		//[CustomCmdHandler("BulletNumTest")]
		//public bool DoBulletNumTest(string[] args)
		//{
		//	if (BulletNumTest.Instance != null)
		//	{
		//		BulletNumTest.Instance.DoTest();
		//		return true;
		//	}

		//	return false;
		//}

		//[CustomCmdHandler("ClearBulletNumTest")]
		//public bool ClearBulletNumTest(string[] args)
		//{
		//	if (BulletNumTest.Instance != null)
		//	{
		//		BulletNumTest.Instance.Clear();
		//		return true;
		//	}

		//	return false;
		//}

		///*
  //      [CustomCmdHandler("MainPlayerName")]
  //      public bool MainPlayerName(string[] args) {
  //          Player player = FamilyMgr.m_myFamily.GetActivePlayer();
  //          Debug.LogFormat(player.name);
  //          return true;
  //      }
		//*/
	}
}
#endif