using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.AssetBundlePatching;

public class BuildAB : Editor
{
    [MenuItem("UnityTools/BuildAssetBundle")]
    static void BuildAssetBundle()
    {
#if UNITY_ANDROID1
        BuildTarget target = BuildTarget.Android;
#elif UNITY_IOS
        BuildTarget target = BuildTarget.iOS;
#elif UNITY_STANDALONE_WIN
        BuildTarget target = BuildTarget.StandaloneWindows;
#elif UNITY_WEBGL
        BuildTarget target = BuildTarget.WebGL;
#else
        BuildTarget target = BuildTarget.StandaloneWindows;
#endif
        string             output = Application.streamingAssetsPath;
        AssetBundleBuild[] abbs   = new AssetBundleBuild[1];
        AssetBundleManifest manifest =
            BuildPipeline.BuildAssetBundles(output + "/sprite.assetbundle", abbs,
                                            BuildAssetBundleOptions.UncompressedAssetBundle, target);
    }
}