/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using UnityEngine;
using XLua;
using GameFramework;

//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class XLuaGenConfig
{
    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {           
                //System
                typeof(System.Reflection.Missing),
                typeof(System.Type),
                typeof(System.Delegate),

                //Unity 
                typeof(UnityEngine.Component),
                typeof(UnityEngine.GameObject),
                typeof(UnityEngine.Transform),
                typeof(UnityEngine.RectTransform),
                typeof(UnityEngine.RectTransform.Axis),
                typeof(UnityEngine.RectTransform.Edge),
                typeof(UnityEngine.UI.Button),
                typeof(UnityEngine.UI.Button.ButtonClickedEvent),
                typeof(UnityEngine.Events.UnityEvent),
                typeof(UnityEngine.UI.Image),
                typeof(UnityEngine.UI.RawImage),
                typeof(UnityEngine.UI.ScrollRect),
                typeof(UnityEngine.UI.Text),
                typeof(UnityEngine.UI.Slider),
                typeof(UnityEngine.UI.Toggle),
                typeof(UnityEngine.UI.Scrollbar),
                typeof(UnityEngine.UI.InputField),
                typeof(UnityEngine.UI.Dropdown),

                
                //Custom
                typeof(System.Action),
                typeof(System.Action<bool>),
                typeof(System.Action<int>),
                typeof(System.Action<float>),
                typeof(System.Action<string>),
                typeof(System.Action<Vector2>),
                typeof(System.Action<Vector3>),
                typeof(System.Action<Vector4>),
                typeof(System.Action<GameObject>),
                typeof(System.Action<Transform>),
                typeof(System.Action<RectTransform>),

                typeof(LuaManager),
            };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
                typeof(System.Action),
                typeof(System.Action<GameObject>),
                typeof(System.Action<Transform>),
                typeof(System.Action<RectTransform>),
                typeof(UnityEngine.Events.UnityAction),

                //typeof(Func<double, double, double>),
                //typeof(Action<string>),
                //typeof(Action<double>),
                //typeof(UnityEngine.Events.UnityAction),
                //typeof(System.Collections.IEnumerator)
    };

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"UnityEngine.WWW", "movie"},
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},

                new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
            };
}
