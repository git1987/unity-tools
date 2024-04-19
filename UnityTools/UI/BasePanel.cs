using UnityEngine;

namespace UnityTools.UI
{
    /// <summary>
    /// UI面板的基类
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        /// <summary>
        /// 面板是否是显示状态
        /// </summary>
        public bool isShow { protected set; get; }
        public virtual string PanelName => GetType().Name;
        /// <summary>
        /// 设置的面板等级
        /// </summary>
        public int panelLv;
        /// <summary>
        /// 当前面板等级
        /// </summary>
        public int currentPanelLv;
        /// <summary>
        /// 面板打开BasePanel，直接调用UIManager.OpenPanel，面板等级==当前面板
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <returns></returns>
        protected P Open<P>(params object[] objs) where P : BasePanel
        {
            UIManager.setPanelLv = false;
            P panel = UIManager.OpenPanel<P>(objs);
            panel.SetPanelLv(currentPanelLv);
            return panel;
        }

        public virtual void SetPanelLv(int panelLv)
        {
            currentPanelLv = panelLv;
            UIManager.SetPanelLv(this, currentPanelLv);
        }
        /// <summary>
        /// 打开面板
        /// </summary>
        public virtual void Show(params object[] objs)
        {
            if (!isShow)
            {
                isShow = true;
                gameObject.SetActive(true);
                UIManager.SetShowPanel(this);
            }
        }
        /// <summary>
        /// 关闭面板
        /// </summary>
        public virtual void Hide()
        {
            if (isShow)
            {
                isShow = false;
                gameObject.SetActive(false);
                UIManager.SetHidePanel(this);
            }
        }
        /// <summary>
        /// 移除面板
        /// </summary>
        public virtual void Disable() { Destroy(this.gameObject); }
    }
}