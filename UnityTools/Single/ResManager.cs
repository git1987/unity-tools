using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityTools;
using UnityTools.Single;
using UnityTools.Extend;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 资源加载管理类，默认情况下根目录是Assets/Res(可以替换成Resources)
/// <para>Prefab路径：Assets/Res/Prefabs</para>
/// <para>特效Prefab路径：Assets/Res/Effects/Prefabs</para>
/// <para>图片资源路径：Assets/Res/Textures</para>
/// <para>材质球资源路径：Assets/Res/Materials</para>
/// <para>UI Prefab路径：Assets/Res/UI/Prefabs</para>
/// <para>UI精灵资源路径：Assets/Res/UI/Sprites</para>
/// <para>UI材质球资源路径：Assets/Res/UI/Materials</para>
/// <para>音效资源路径：Assets/Res/Audios</para>
/// </summary>
public class ResManager : SingleMono<ResManager>
{
    private static bool isResources;
#if UNITY_EDITOR
    private static bool isEditor = true;
#endif
    public class AssetPath
    {
        /// <summary>
        /// 资源根目录
        /// </summary>
        public string basePath { private set; get; }
        public string prefabPath;
        public string effectPrefabPath;
        public string texturePath;
        public string materialPath;
        public string uiPrefabPath;
        public string uiSpritePath;
        public string uiMaterialPath;
        public string audioPath;
        public AssetPath(string basePath)
        {
            this.basePath = basePath;
        }
    }

    public static AssetPath assetPath = null;
    #region 静态方法
    public static void SetAssetPath()
    {
        AssetPath ap = new AssetPath("Assets/Res")
        {
            prefabPath       = "Prefabs",
            effectPrefabPath = "Effects/Prefabs",
            texturePath      = "Textures",
            materialPath     = "Materials",
            uiPrefabPath     = "UI/Prefabs",
            uiSpritePath     = "UI/Sprites",
            uiMaterialPath   = "UI/Materials",
            audioPath        = "Audios",
        };
        SetAssetPath(ap);
    }
    public static void SetAssetPath(AssetPath _assetPath)
    {
        if (assetPath == null)
            assetPath = _assetPath;
        else
            Debuger.LogError("重复调用了SetAssetPath方法");
    }
    #endregion
    Dictionary<string, AssetBundle> abs = new Dictionary<string, AssetBundle>();
    Dictionary<string, GameObject> prefabs;
    Dictionary<string, GameObject> uiPrefabs;
    Dictionary<string, GameObject> effectPrefabs;
    Dictionary<string, Material> materials;
    Dictionary<string, Sprite> sprites;
    Dictionary<string, Texture> textures;
    Dictionary<string, AudioClip> audios;
    protected override void Awake()
    {
        base.Awake();
        if (assetPath == null)
        {
            Debuger.LogError("没有调用SetAssetPath方法!");
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.Clear();
    }
    public void Clear()
    {
        prefabs?.Clear();
        uiPrefabs?.Clear();
        effectPrefabs?.Clear();
        materials?.Clear();
        sprites?.Clear();
        textures?.Clear();
        audios?.Clear();
        abs?.ForAction((name, ab) => ab?.Unload(true));
        abs?.Clear();
    }
    private AssetBundle LoadAB(string abName)
    {
        if (abs.TryGetValue(abName, out AssetBundle ab))
        {
            return ab;
        }
        ab = AssetBundle.LoadFromFile(abName + ".assetbundle");
        abs.Add(abName, ab);
        AssetBundleManifest abm = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        foreach (string abPath in abm.GetAllDependencies(abName))
        {
            LoadAB(abPath);
            //UnityTools.Debuger.Log(abPath);
        }
        return ab;
    }
    private GameObject _GetPrefab(string path, string prefabName)
    {
        GameObject prefab = null;
        if (isResources)
        {
            prefab = Resources.Load<GameObject>($"{assetPath.prefabPath}/{prefabName}");
        }
#if UNITY_EDITOR
        else if (isEditor)
        {
            prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>($"{assetPath.basePath}/{assetPath.prefabPath}{prefabName}");
        }
        else
#endif
        {
            if (prefabs == null)
            {
                prefabs = new Dictionary<string, GameObject>();
                AssetBundle ab = LoadAB("prefab");
                foreach (GameObject go in ab.LoadAllAssets<GameObject>())
                {
                    prefabs.Add(go.name, go);
                    if (prefabName == go.name)
                    {
                        prefab = go;
                    }
                }
            }
            else
            {
                prefabs.TryGetValue(prefabName, out prefab);
            }
        }
        if (prefab == null) UnityTools.Debuger.LogError($"[{prefabName}]加载失败");
        return prefab;
    }
}