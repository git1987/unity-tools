using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace UnityTools.Editor
{
    public class Windows : EditorWindow
    {
        [MenuItem("UnityTools/Windows")]
        private static void OpenWindows()
        {
            Windows windows = EditorWindow.GetWindow<Windows>("功能窗口");
        }
        private bool inWindows;
        Rect inWindowsRact = new Rect(new Vector2(100, 100), new Vector2(200, 200));
        private string csharpFilePath;
        private string panelName;
        private string menuName;
        private void OnGUI()
        {
            GUILayout.Space(50);
            csharpFilePath = EditorGUILayout.TextField("脚本导出路径",csharpFilePath);
            if (GUILayout.Button("选择脚本路径"))
            {
                csharpFilePath = EditorUtility.OpenFolderPanel("选择面板导出的路径", "Assets", string.Empty);
                if (EditorTools.InUnityProject(csharpFilePath)) { }
            }
            panelName = EditorGUILayout.TextField("面板名称", panelName);
            menuName  = EditorGUILayout.TextField("Component Name", menuName);
            //根据名称创建面板类
            if (GUILayout.Button("创建面板脚本结构"))
            {
                if (HasPanel(panelName))
                {
                    Debug.LogError($"面板：{panelName}已经存在");
                }
                else
                {
                    CreatePanelCSharpFile(csharpFilePath, panelName, menuName);
                }
            }
            inWindows = GUILayout.Toggle(inWindows, "窗中窗");
            if (inWindows)
            {
                InWindows();
            }
        }
        //UnityTools程序集内是否包含这个面板类
        bool HasPanel(string panelName)
        {
            string assemblyName = "UnityTools";
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (assembly != null)
            {
                if (assembly.GetType($"{panelName}Panel") != null)
                {
                    return true;
                }
            }
            else
            {
                Debug.LogError($"不存在程序集{assemblyName}");
            }
            return false;
        }
        //根据路径创建面板的子类
        void CreatePanelCSharpFile(string path, string panelName, string menuName)
        {
            string left = "{", right = "}", sign = "\"";
            if (!Directory.Exists($"{path}/{panelName}Panel"))
            {
                Directory.CreateDirectory($"{path}/{panelName}Panel");
            }
            //Panel
            using (FileStream stream = new FileStream($"{path}/{panelName}Panel/{panelName}Panel.cs", FileMode.Create,
                       FileAccess.Write
                   ))
            {
                string menu = (menuName is { Length: > 0 }) ? menuName : (panelName + "Panel");
                string code = @$"using UnityEngine;
using UnityTools.UI;

[AddComponentMenu({sign}UnityTools/UI/{menu}{sign})]
public class {panelName}Panel : BasePanel
{left}

{right}";
                StreamWriter writer = new(stream);
                writer.Write(code);
                //清空缓冲区
                writer.Flush();
                //关闭流
                writer.Close();
                //关闭文件
                stream.Close();
            }
            //Model
            using (FileStream stream = new FileStream($"{path}/{panelName}Panel/{panelName}Model.cs", FileMode.Create,
                       FileAccess.Write
                   ))
            {
                string code = @$"using UnityTools.UI;
public class {panelName}Model : BaseModel
{left}
    private static {panelName}Model _instance;
    public static {panelName}Model instance
    {left}
        get
        {left}
            if (_instance == null) _instance = new();
            return _instance;
        {right}
    {right}
    private {panelName}Model()
    {left}
        CreateModel(this);
    {right}
    protected override void Disable()
    {left}
        _instance = null;
    {right}
{right}";
                StreamWriter writer = new(stream);
                writer.Write(code);
                //清空缓冲区
                writer.Flush();
                //关闭流
                writer.Close();
                //关闭文件
                stream.Close();
            }
            AssetDatabase.Refresh();
        }
        //窗中窗
        void InWindows()
        {
            BeginWindows();
            inWindowsRact = GUI.Window(1, inWindowsRact, id =>
                {
                    if (GUILayout.Button("按钮"))
                    {
                        Debug.Log("窗中窗的按钮");
                    }
                    GUI.DragWindow();
                }, "窗中窗"
            );
            EndWindows();
        }
    }
}