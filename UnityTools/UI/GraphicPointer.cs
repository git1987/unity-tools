using UnityEngine;
using UnityEngine.EventSystems;
namespace UnityTools.UI
{
    /// <summary>
    /// Graphic检测鼠标操作的监听事件
    /// </summary>
    public class GraphicPointer : MonoBehaviour,
                                    IPointerEnterHandler,
                                    IPointerExitHandler,
                                    IPointerClickHandler,
                                    IPointerDownHandler,
                                    IPointerUpHandler
    {
        //鼠标点击类型
        public enum MouseClickType
        {
            Left,
            Middle,
            Right
        }

        MouseClickType clickType;
        EventAction enterAction,
                    exitAction,
                    clickAction,
                    downAction_Left,
                    upAction_Left,
                    downAction_Middle,
                    upAction_Middle,
                    downAction_Right,
                    upAction_Right;
        public void SetEnterAction(EventAction enter) { enterAction = enter; }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerEnter == this.gameObject) enterAction?.Invoke();
        }
        public void SetExitAction(EventAction exit) { exitAction = exit; }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerEnter == this.gameObject) exitAction?.Invoke();
        }
        public void SetClickAction(EventAction click) { clickAction = click; }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.pointerEnter == this.gameObject) clickAction?.Invoke();
        }
        public void SetDownAction(EventAction down, MouseClickType clickType = MouseClickType.Left)
        {
            this.clickType = clickType;
            if (clickType == MouseClickType.Left)
            {
                downAction_Left = down;
            }
            else if (clickType == MouseClickType.Middle)
            {
                downAction_Middle = down;
            }
            else if (clickType == MouseClickType.Right)
            {
                downAction_Right = down;
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            switch (clickType)
            {
                case MouseClickType.Left:
                    // if (Config.leftMouseDown) { downAction_Left?.Invoke(); }
                    //左键点击UI不需要判断mouse
                    downAction_Left?.Invoke();
                    break;
                case MouseClickType.Middle:
                    if (Config.middleMouseDown)
                    {
                        downAction_Middle?.Invoke();
                    }
                    break;
                case MouseClickType.Right:
                    if (Config.rightMouseDown)
                    {
                        downAction_Right?.Invoke();
                    }
                    break;
            }
        }
        public void SetUpAction(EventAction up, MouseClickType clickType = MouseClickType.Left)
        {
            if (clickType == MouseClickType.Left)
                upAction_Left = up;
            else if (clickType == MouseClickType.Middle)
                upAction_Middle = up;
            else if (clickType == MouseClickType.Right)
                upAction_Right = up;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            switch (clickType)
            {
                case MouseClickType.Left:
                    // if (Config.leftMouseUp) { upAction_Left?.Invoke(); }
                    //左键点击UI不需要判断mouse
                    upAction_Left?.Invoke();
                    break;
                case MouseClickType.Middle:
                    if (Config.middleMouseUp)
                    {
                        upAction_Middle?.Invoke();
                    }
                    break;
                case MouseClickType.Right:
                    if (Config.rightMouseUp)
                    {
                        upAction_Right?.Invoke();
                    }
                    break;
            }
        }
    }
}