#if UNITY_EDITOR_OSX
// This class runs on editor startup and enforces executable permissions for the phantomjs binary on mac

using UnityEngine;
using UnityEditor;
using System.Diagnostics;

[InitializeOnLoad]
public static class Install_PhantomJS {
    static Install_PhantomJS()
    {
        Process chmod = new Process();
        chmod.StartInfo.UseShellExecute = false;
        chmod.StartInfo.WorkingDirectory = Application.dataPath + "/Cradle/Editor/ThirdParty/PhantomJS/bin/osx";
        chmod.StartInfo.FileName = "chmod";
        chmod.StartInfo.Arguments = "+x phantomjs";
        chmod.StartInfo.RedirectStandardOutput = true;
        chmod.Start();
        chmod.WaitForExit();
    }
}

#endif