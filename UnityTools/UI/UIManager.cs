using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Extend;

namespace UnityTools.UI
{
    /// <summary>
    /// UI总控制器
    /// </summary>
    public sealed class UIManager
    {
        /// <summary>
        /// 当前场景中的所有 UIPanel集合
        /// </summary>
        private static readonly Dictionary<string, BasePanel> panels = new Dictionary<string, BasePanel>();
        /// <summary>
        /// 当前场景的UICtrl
        /// </summary>
        public static UICtrl uiCtrl { private set; get; }
        /// <summary>
        /// 根据面板名称的全称自动加载panel prefab的委托
        /// </summary>
        private static EventFunction<string, GameObject> getPanelPrefabFunction = null;
        /// <summary>
        /// 根据面板名称的获取已经实例化的panel的委托
        /// </summary>
        private static EventFunction<string, GameObject> getPanelFunction = null;
        /// <summary>
        /// 已经显示的panel
        /// </summary>
        private static Dictionary<string, BasePanel> showPanels = new();
        /// <summary>
        /// 是否设置面板级别
        /// </summary>
        public static bool setPanelLv = true;
        /// <summary>
        /// 设置自动加载panel prefab的委托
        /// </summary>
        /// <param name="function"></param>
        public static void SetBuildPanelFunction(EventFunction<string, GameObject> function)
        {
            getPanelPrefabFunction = function;
        }
        /// <summary>
        /// 设置获取panel实例的委托
        /// </summary>
        /// <param name="function"></param>
        public static void SetGetPanelFunction(EventFunction<string, GameObject> function)
        {
            getPanelFunction = function;
        }
        /// <summary>
        /// 移除面板时的委托
        /// </summary>
        private static EventAction<string> removePanelAction = null;
        /// <summary>
        /// 设置自动加载panel prefab的委托
        /// </summary>
        /// <param name="function"></param>
        public static void SetRemovePanelAction(EventAction<string> action)
        {
            removePanelAction = action;
        }
        /// <summary>
        /// 设置当前场景的UICtrl
        /// </summary>
        /// <param name="_uiCtrl"></param>
        public static void SetUICtrl(UICtrl _uiCtrl)
        {
            if (uiCtrl == null)
            {
                //设置UICtrl时进入到新场景，将旧的面板词典清空
                panels.Clear();
            }
            else { Debuger.LogError($"之前场景没有清空UICtrl[{uiCtrl}]", uiCtrl.gameObject); }
            uiCtrl = _uiCtrl;
        }
        /// <summary>
        /// 移除场景UICtrl控制器
        /// </summary>
        /// <param name="currentUICtrl"></param>
        public static void RemoveUICtrl(UICtrl currentUICtrl)
        {
            if (currentUICtrl == null) { Debuger.LogError("currentUICtrl is null"); }
            else if (uiCtrl == null) { Debuger.LogError("UIManager.uiCtrl是空的，当前场景没有注册UICtrl"); }
            else if (uiCtrl != currentUICtrl) { Debuger.LogError("当前场景uiCtrl和UIManger.uiCtrl不相同"); }
            uiCtrl = null;
            BaseModel.ClearModel();
        }
        /// <summary>
        /// 设置面板为显示状态
        /// </summary>
        /// <param name="panel"></param>
        public static void SetShowPanel(BasePanel panel)
        {
            if (!showPanels.ContainsKey(panel.PanelName))
            {
                showPanels.Add(panel.PanelName, panel);
            }
        }
        /// <summary>
        /// 设置面板为关闭状态
        /// </summary>
        /// <param name="panel"></param>
        public static void SetHidePanel(BasePanel panel)
        {
            if (showPanels.ContainsKey(panel.PanelName))
            {
                showPanels.Remove(panel.PanelName);
            }
        }
        /// <summary>
        /// 泛型获取当前场景的UICtrl
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <returns></returns>
        public static C GetUICtrl<C>() where C : UICtrl
        {
            if (uiCtrl is C) { return uiCtrl as C; }
            else
            {
                Debuger.LogError($"场景中的UICtrl[{uiCtrl.GetType().Name}]不是{typeof(C).Name}");
                return null;
            }
        }
        /// <summary>
        /// 创建面板
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="panelObj"></param>
        public static BasePanel CreatePanel(GameObject panelObj)
        {
            if (panelObj == null) { throw new NullReferenceException($"panel GameObejct is null!"); }
            string panelName = panelObj.name;
            BasePanel p;
            if (panels.TryGetValue(panelName, out BasePanel basePanel))
            {
                p = basePanel;
            }
            else
            {
                //GameObject panelObj = GameObject.Instantiate<GameObject>(panelObj);
                panelObj.SetActive(false);
                p = panelObj.GetComponent<BasePanel>();
                if (p == null) { throw new NullReferenceException($"[{panelName}] Component is null!"); }
                panels.Add(panelName, p);
                RectTransform rect = panelObj.GetComponent<RectTransform>();
                rect.SetParentReset(uiCtrl.rect);
                Tools.RectTransformSetSurround(rect);
            }
            return p;
        }
        /// <summary>
        /// 打开面板
        /// </summary>
        /// <typeparam name="P">面板类</typeparam>
        /// <param name="panelLv">设置的面板等级(0级为预留最低层级)</param>
        /// <returns></returns>
        public static P OpenPanel<P>(params object[] objs) where P : BasePanel
        {
            string panelName = typeof(P).Name;
            P panel = OpenPanel(panelName, objs) as P;
            if (panel == null)
            {
                Debuger.LogError($"{panelName}不存在");
            }
            return panel;
        }
        /// <summary>
        /// 根据面板名称打开面板
        /// </summary>
        /// <param name="panelName"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static BasePanel OpenPanel(string panelName, params object[] objs)
        {
            BasePanel panel;
            if (!panels.TryGetValue(panelName, out panel))
            {
                GameObject panelObj = null;
                if (getPanelPrefabFunction != null)
                    panelObj = GameObject.Instantiate(getPanelPrefabFunction(panelName));
                else if (getPanelFunction != null)
                    panelObj = getPanelFunction(panelName);
                if (panelObj == null)
                    throw new NullReferenceException($"[{panelName}] GameObejct is null!");
                panelObj.name = panelName;
                panel = CreatePanel(panelObj);
            }
            panel.Show(objs);
            if (setPanelLv)
            {
                panel.SetPanelLv(panel.panelLv);
            }
            else
                setPanelLv = true;
            return panel;
        }
        /// <summary>
        /// 关闭面板
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="p"></param>
        public static void ClosePanel<P>() where P : BasePanel
        {
            ClosePanel(typeof(P).Name);
        }

        public static void ClosePanel(string panelName)
        {
            if (panels.TryGetValue(panelName, out BasePanel panel))
            {
                SetHidePanel(panel);
            }
        }
        /// <summary>
        /// 获取面板
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <returns></returns>
        public static P GetPanel<P>() where P : BasePanel
        {
            string panelName = typeof(P).Name;
            if (panels.ContainsKey(panelName)) return panels[panelName] as P;
            Debuger.LogFormat("没有初始化{0}面板", panelName);
            return null;
        }

        /// <summary>
        /// 获取面板
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <returns></returns>
        public static BasePanel GetPanel(string panelName)
        {
            panels.TryGetValue(panelName, out BasePanel panel);
            if (panel == null) Debuger.LogError($"没有初始化{0}面板");
            return panel;
        }
        /// <summary>
        /// 判断面板是否打开中
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <returns></returns>
        public static bool IsOpen<P>()
        {
            return IsOpen(typeof(P).Name);
        }
        /// <summary>
        /// 判断面板是否打开中
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <returns></returns>
        public static bool IsOpen(string panelName)
        {
            return showPanels.ContainsKey(panelName);
        }
        /// <summary>
        /// 移除面板
        /// </summary>
        /// <typeparam name="P"></typeparam>
        public static void RemovePanel<P>() where P : BasePanel
        {
            string panelName = typeof(P).Name;
            RemovePanel(panelName);
        }
        /// <summary>
        /// 移除面板
        /// </summary>
        /// <typeparam name="P"></typeparam>
        public static void RemovePanel(string panelName)
        {
            if (panels.TryGetValue(panelName, out BasePanel panel))
            {
                panel.Disable();
                removePanelAction?.Invoke(panelName);
                panels.Remove(panelName);
            }
        }
        /// <summary>
        /// 设置面板等级
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="panelLv"></param>
        public static void SetPanelLv(BasePanel panel, int panelLv)
        {
            Transform panelParent = GetPanelParent(panelLv);
            panel.transform.SetParent(panelParent);
            panel.transform.SetAsLastSibling();
        }
        static RectTransform GetPanelParent(int panelLv)
        {
            Transform panelParent = uiCtrl.rect.Find("Panel" + panelLv);
            if (panelParent == null)
            {
                int index = 0;
                while (index <= panelLv)
                {
                    if (index == panelLv || uiCtrl.rect.Find("Panel" + index) == null)
                    {
                        GameObject go = new GameObject("Panel" + index);
                        RectTransform rect = go.AddComponent<RectTransform>();
                        rect.SetParentReset(uiCtrl.rect);
                        Tools.RectTransformSetSurround(rect);
                    }
                    index++;
                }
                panelParent = uiCtrl.rect.Find("Panel" + panelLv);
            }
            if (panelParent == null) Debuger.LogError("面板层级错误：" + panelLv);
            return panelParent as RectTransform;
        }
    }
}