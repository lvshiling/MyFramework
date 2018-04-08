using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public class GenProto
{
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static public void Recursive( string path ) {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names) {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs) {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    [MenuItem("Tools/构建proto.lua")]
    public static void BuildProtobufFile()
    {
        string dir = Application.dataPath.ToLower() + "/LuaScript/PbLua";
        paths.Clear(); files.Clear(); Recursive( dir );

        //"D:/protoc-3.5.1-win32/protoc.exe";
        string protoc = Application.dataPath.Replace( "Assets", "Tools/protoc.exe" );
        //"\"d:/protoc-gen-lua-master/plugin/protoc-gen-lua.bat\""
        string protoc_gen_lua = Application.dataPath.Replace( "Assets", "Tools/protoc-gen-lua-master/plugin/protoc-gen-lua.bat" );
        string protoc_gen_dir = string.Format( "\"{0}\"", protoc_gen_lua );

        foreach (string f in files) {
            string name = Path.GetFileName(f);
            string ext = Path.GetExtension(f);
            if (!ext.Equals(".proto")) continue;

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = protoc;
            info.Arguments = " --lua_out=./ --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name;
            UnityEngine.Debug.Log( info.Arguments );
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.UseShellExecute = true;
            info.WorkingDirectory = dir;
            info.ErrorDialog = true;
            UnityEngine.Debug.Log(info.FileName + " " + info.Arguments);

            Process pro = Process.Start(info);
            pro.WaitForExit();
        }
        AssetDatabase.Refresh();
    }

    [MenuItem( "Tools/构建proto.pb" )]
    public static void BuildProtobufPbFile()
    {
        string dir = Application.dataPath.Replace( "Assets", "Tools/Proto/LuaProto" );
        paths.Clear(); files.Clear(); Recursive( dir );

        string outDir = Application.dataPath + "/LuaScript/PbLua/";

        //"D:/protoc-3.5.1-win32/protoc.exe";
        string protoc = Application.dataPath.Replace( "Assets", "Tools/protoc.exe" );

        foreach( string f in files )
        {
            string name = Path.GetFileName( f );
            string ext = Path.GetExtension( f );
            if( !ext.Equals( ".proto" ) ) continue;

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = protoc;
            info.Arguments = "--descriptor_set_out=" + outDir + name.Replace( ".proto", ".pb.bytes" ) + " " + name;
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.UseShellExecute = true;
            info.WorkingDirectory = dir;
            info.ErrorDialog = true;
            UnityEngine.Debug.Log( info.FileName + " " + info.Arguments );

            Process pro = Process.Start( info );
            pro.WaitForExit();
        }
        AssetDatabase.Refresh();
    }
}