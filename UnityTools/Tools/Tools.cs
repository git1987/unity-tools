using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTools
{
    /// <summary>
    /// 工具类静态方法
    /// </summary>
    public sealed class Tools
    {
        /// <summary>
        /// 指定tran的轴向朝向目标点的Quaternion
        /// </summary>
        /// <param name="tran">Transform</param>
        /// <param name="target">目标点</param>
        /// <param name="axis">朝向目标点的轴向</param>
        /// <param name="relativeTo">坐标系</param>
        /// <returns></returns>
        public static Quaternion AxisLookAt(Transform tran, Vector3 target, Vector3 axis, Space relativeTo = Space.Self)
        {
            Vector3 targetDir = target - tran.position;
            //指定哪根轴朝向目标,自行修改Vector3的方向
            Vector3 fromDir;
            if (relativeTo == Space.Self)
                fromDir = tran.rotation * axis;
            else
                fromDir = axis;
            //计算垂直于当前方向和目标方向的轴
            Vector3 direction = Vector3.Cross(fromDir, targetDir).normalized;
            //计算当前方向和目标方向的夹角
            float angle = Vector3.Angle(fromDir, targetDir);
            //将当前朝向向目标方向旋转一定角度，这个角度值可以做插值
            return Quaternion.AngleAxis(angle, direction) * tran.rotation;
        }
        /// <summary>
        /// 设置RectTransform四周围绕适配
        /// </summary>
        /// <param name="rect"></param>
        public static void RectTransformSetSurround(RectTransform rect)
        {
            rect.anchorMin          = Vector2.zero;
            rect.anchorMax          = Vector2.one;
            rect.offsetMin          = Vector2.zero;
            rect.offsetMax          = Vector2.zero;
            rect.anchoredPosition3D = Vector3.zero;
            rect.sizeDelta          = Vector2.zero;
            rect.localScale         = Vector3.one;
            rect.localRotation      = Quaternion.identity;
        }
        /// <summary>
        /// 设置文字：Get所有显示文字的组件（Text,TMP_Text,TextMesh）
        /// </summary>
        public static void SetText(Transform transform, string content, Color? textColor = null)
        {
            SetText(transform.gameObject, content, textColor);
        }
        /// <summary>
        /// 设置文字：Get所有显示文字的组件（Text,TMP_Text,TextMesh）
        /// </summary>
        public static void SetText(GameObject gameObject, string content, Color? textColor = null)
        {
            var text = gameObject.GetComponent<Text>();
            // if (content.IndexOf("\\n") > -1)
            // {
            //     UnityTools.Debuger.Log("有换行：" + content, transform.gameObject);
            // }
            //清空转义字符
            // content = Regex.Unescape(content);
            if (text == null)
            {
                TextMesh textMesh = gameObject.GetComponent<TextMesh>();
                if (textMesh == null)
                {
                    TMPro.TMP_Text textPro = gameObject.GetComponent<TMPro.TMP_Text>();
                    if (textPro == null)
                    {
                        Debuger.LogError("不存在任何Text渲染组件！！");
                    }
                    else
                    {
                        textPro.text = content;
                        if (textColor != null) { textPro.color = textColor.GetValueOrDefault(); }
                    }
                }
                else
                {
                    textMesh.text = content;
                    if (textColor != null) { textMesh.color = textColor.GetValueOrDefault(); }
                }
            }
            else
            {
                text.text = content;
                if (textColor != null) { text.color = textColor.GetValueOrDefault(); }
            }
        }
        /// <summary>
        /// 设置文字富文本颜色和大小
        /// </summary>
        /// <param name="content"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string SetTextColor(string content, string colorHtml = "ffffffff", uint size = 0)
        {
            string text = $"<color=#{colorHtml}>{content}</color>";
            if (size > 0)
            {
                text = $"<size={size}>{text}</size>";
            }
            return text;
        }
        /// <summary>
        /// 一半的成功率
        /// </summary>
        /// <returns></returns>
        public static bool ProbabilityHalf()
        {
            return Probability100(50);
        }
        /// <summary>
        /// 整数百分比成功率
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static bool Probability100(int rate)
        {
            if (rate >= 100) return true;
            int temp = Random.Range(0, 100);
            if (temp < rate) return true;
            return false;
        }
        /// <summary>
        /// 从List中随机挑选count个元素返回新的List
        /// </summary>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public List<T> GetRandomList<T>(List<T> list, int count)
        {
            if (list.Count <= count)
                return list;
            else
            {
                List<T> list2 = new List<T>(list);
                List<T> tempList = new List<T>();
                while (tempList.Count < count && list2.Count > 0)
                {
                    int index = Random.Range(0, list2.Count);
                    tempList.Add(list2[index]);
                    list2.RemoveAt(index);
                }
                return tempList;
            }
        }
    }
}