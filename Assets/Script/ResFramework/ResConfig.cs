using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ResFramework
{
    public class ResConfig
    {
        public string BundleName = string.Empty;

        public Int64 Size = 0;

        public string Md5 = string.Empty;

        public uint Version = 0;

        public List<string> Dependencies = new List<string>();

        public List<string> Assets = new List<string>();
    }
}
