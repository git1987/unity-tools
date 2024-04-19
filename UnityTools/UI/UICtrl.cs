using UnityEngine;
using UnityEngine.UI;
using UnityTools.Extend;
namespace UnityTools.UI
{
    /// <summary>
    /// 场景UI控制器
    /// </summary>
    public abstract class UICtrl : MonoBehaviour
    {
        /// <summary>
        /// 遮罩：UI阻挡点击判断
        /// </summary>
        private RectTransform maskRect;
        /// <summary>
        /// 场景的Canvas
        /// </summary>
        public Canvas canvas { private set; get; }
        /// <summary>
        /// Canvas的RectTransform
        /// </summary>
        public RectTransform rect { private set; get; }
        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();
            rect = transform as RectTransform;
            GameObject mask = new GameObject("mask");
            mask.transform.SetParentReset(rect);
            maskRect = mask.AddComponent<RectTransform>();
            Image image = mask.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
            Tools.RectTransformSetSurround(maskRect);
            mask.SetActive(false);
            UIManager.SetUICtrl(this);
        }
        /// <summary>
        /// 先实现子类的Start方法，最后调用父类base.Start()
        /// </summary>
        protected virtual void Start()
        {
            GameBegin();
        }
        protected virtual void OnDestroy()
        {
            UIManager.RemoveUICtrl(this);
        }
        /// <summary>
        /// 动态加载场景时，关闭UICtrl.Start方法，手动调用GameBegin()
        /// </summary> 
        public void GameBegin()
        {
            Init();
        }
        /// <summary>
        /// 初始化UICtrl，由GameBegin调用
        /// </summary>
        protected abstract void Init();
        /// <summary>
        /// UI控制内打开面板
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="panelLv"></param>
        protected virtual P OpenPanel<P>(int panelLv = 1) where P : BasePanel
        {
            return UIManager.OpenPanel<P>(panelLv);
        }
        /// <summary>
        /// 设置遮罩状态:过度一些动画，防止在动画中点击触发了事件
        /// </summary>
        /// <param name="active"></param>
        public virtual void SetMask(bool active)
        {
            maskRect.gameObject.SetActive(active);
            if (active)
            {
                maskRect.SetAsLastSibling();
            }
        }
    }
}
