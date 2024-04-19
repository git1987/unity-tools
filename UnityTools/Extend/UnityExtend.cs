using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Single;
namespace UnityTools.Extend
{
    /// <summary>
    /// UnityEngine命名空间下类扩展方法
    /// </summary>
    public static class UnityEngineExtend
    {
        #region GameObject
        /// <summary>
        /// 获取Component，如果没有则Add一个
        /// </summary>
        /// <returns>UnityEngine.Component</returns>
        public static Component MateComponent(this GameObject go, Type type)
        {
            UnityEngine.Component t = go.GetComponent(type);
            if (t == null) t = go.AddComponent(type);
            return t;
        }
        /// <summary>
        /// 获取Component，如果没有则Add一个
        /// </summary>
        /// <returns>T</returns>
        public static T MateComponent<T>(this GameObject go) where T : Component
        {
            T t = go.GetComponent<T>();
            if (t == null) t = go.AddComponent<T>();
            return t;
        }
        #endregion
        #region Transform
        /// <summary>
        /// transform，如果是RectTransform，也重置anchoredPosition3D
        /// </summary>
        /// <param name="transform">Transform</param>
        /// <param name="parent">要设置的父级Transform</param>
        public static void SetParentReset(this Transform transform, Transform parent)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            if (transform is RectTransform) ((RectTransform)transform).anchoredPosition3D = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        /// <summary>
        /// 根据名字获取子级的transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Transform GetChildByName(this Transform transform, string name)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name == name) return child;
                child = child.GetChildByName(name);
                if (child != null) return child;
            }
            return null;
        }
        /// <summary>
        /// 遍历transform的子级
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="action">将子级作为参数的回调</param>
        /// <param name="includeGrandchildren">是否包含孙集</param>
        public static void ForAction(this Transform transform, EventAction<Transform> action,
                                     bool includeGrandchildren = false)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                action?.Invoke(child);
                if (includeGrandchildren)
                {
                    child.ForAction(action, includeGrandchildren);
                }
            }
        }
        /// <summary>
        /// 移除Transform的所有子级：释放到Pool内
        /// </summary>
        /// <param name="transform"></param>
        public static void RemoveChild(this Transform transform)
        {
            if (GameObjectPool.instance != null)
            {
                if (GameObjectPool.instance.transform == transform)
                {
                    Debuger.LogError("不能对Pool的transform清除子级");
                    return;
                }
            }
            while (transform.childCount > 0)
            {
                //先移除父级，再Dsstroy
                GameObject child = transform.GetChild(0).gameObject;
                child.transform.SetParent(null);
                GameObjectPool.Recover(child);
            }
        }
        #endregion
    }
    public static class ClassExtend
    {
        #region string
        public static int ToInt(this string str, int defaultInt = 0)
        {
            if (int.TryParse(str, out int i))
            {
                return i;
            }
            return defaultInt;
        }
        public static float ToFloat(this string str, float defaultFloat = 0)
        {
            if (float.TryParse(str, out float f))
            {
                return f;
            }
            return defaultFloat;
        }
        public static long ToLong(this string str, long defaultLong = 0)
        {
            if (long.TryParse(str, out long l))
            {
                return l;
            }
            return defaultLong;
        }
        #endregion
        #region List
        /// <summary>
        /// 遍历List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action">传参list中的item的回调</param>
        public static void ForAction<T>(this List<T> list, EventAction<T> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action?.Invoke(list[i]);
            }
        }
        /// <summary>
        /// 遍历List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action">传参list中的item和index的回调</param>
        public static void ForAction<T>(this List<T> list, EventAction<T, int> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action?.Invoke(list[i], i);
            }
        }
        /// <summary>
        /// 遍历List：通过breakFunc返回值委托判断是否从循环遍历中跳出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action">传参list中的item的回调</param>
        /// <param name="breakFunc">判断是否从遍历中break的bool返回值委托</param>
        public static void ForAction<T>(this List<T> list, EventAction<T> action, EventFunction<bool> breakFunc)
        {
            if (breakFunc == null)
                list.ForAction(action);
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    action?.Invoke(list[i]);
                    if (breakFunc.Invoke()) break;
                }
            }
        }
        /// <summary>
        /// 遍历List：通过breakFunc返回值委托判断是否从循环遍历中跳出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action">传参list中的item和index的回调</param>
        /// <param name="breakFunc">判断是否从遍历中break的bool返回值委托</param>
        public static void ForAction<T>(this List<T> list, EventAction<T, int> action, EventFunction<bool> breakFunc)
        {
            if (breakFunc == null)
                list.ForAction(action);
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    action?.Invoke(list[i], i);
                    if (breakFunc.Invoke()) break;
                }
            }
        }
        /// <summary>
        /// 遍历List：通过isBreak变量判断是否从循环遍历中跳出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action"></param>
        /// <param name="isBreak">满足break条件的bool变量</param>
        public static void ForAction<T>(this List<T> list, EventAction<T> action, in bool isBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action?.Invoke(list[i]);
                if (isBreak) break;
            }
        }
        /// <summary>
        /// 遍历List：通过isBreak变量判断是否冲循环遍历中跳出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action"></param>
        /// <param name="isBreak">满足break条件的bool变量</param>
        public static void ForAction<T>(this List<T> list, EventAction<T, int> action, in bool isBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action?.Invoke(list[i], i);
                if (isBreak) break;
            }
        }
        #endregion
        #region Dictionary
        /// <summary>
        /// 遍历Dictionary
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="action"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        public static void ForAction<K, V>(this Dictionary<K, V> dic, EventAction<K, V> action)
        {
            if (dic.Count == 0) return;
            List<K> kList = new List<K>(dic.Keys);
            List<V> vList = new List<V>(dic.Values);
            for (int i = 0; i < kList.Count; i++)
            {
                action?.Invoke(kList[i], vList[i]);
            }
        }
        /// <summary>
        /// 遍历Dictionary：通过breakAction回调判断是否冲循环遍历中跳出
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="action"></param>
        /// <param name="breakAction">跳出遍历回调</param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        public static void ForAction<K, V>(this Dictionary<K, V> dic, EventAction<K, V> action,
                                           EventFunction<bool> breakAction)
        {
            if (dic.Count == 0) return;
            if (breakAction == null)
            {
                dic.ForAction(action);
            }
            else
            {
                List<K> kList = new List<K>(dic.Keys);
                List<V> vList = new List<V>(dic.Values);
                for (int i = 0; i < kList.Count; i++)
                {
                    action?.Invoke(kList[i], vList[i]);
                    if (breakAction.Invoke()) break;
                }
            }
        }
        /// <summary>
        /// 遍历Dictionary：通过isBreak变量判断是否冲循环遍历中跳出
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="action"></param>
        /// <param name="isBreak">跳出遍历</param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        public static void ForAction<K, V>(this Dictionary<K, V> dic, EventAction<K, V> action, in bool isBreak)
        {
            if (dic.Count == 0) return;
            List<K> kList = new List<K>(dic.Keys);
            List<V> vList = new List<V>(dic.Values);
            for (int i = 0; i < kList.Count; i++)
            {
                action?.Invoke(kList[i], vList[i]);
                if (isBreak) break;
            }
        }
        #endregion
    }
}