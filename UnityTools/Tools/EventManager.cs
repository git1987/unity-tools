using System;
using System.Collections.Generic;
using UnityTools.Extend;

namespace UnityTools
{
    #region 委托类型
    /// <summary>
    /// 无参无返回值委托
    /// </summary>
    public delegate void EventAction();
    /// <summary>
    /// 1参无返回值委托
    /// </summary>
    public delegate void EventAction<A>(A a);
    /// <summary>
    /// 2参无返回值委托
    /// </summary>
    public delegate void EventAction<A, B>(A a, B b);
    /// <summary>
    /// 3参无返回值委托
    /// </summary>
    public delegate void EventAction<A, B, C>(A a, B b, C c);
    /// <summary>
    /// 4参无返回值委托
    /// </summary>
    public delegate void EventAction<A, B, C, D>(A a, B b, C c, D d);
    /// <summary>
    /// 5参无返回值委托
    /// </summary>
    public delegate void EventAction<A, B, C, D, E>(A a, B b, C c, D d, E e);
    /// <summary>
    /// 6参无返回值委托
    /// </summary>
    public delegate void EventAction<A, B, C, D, E, F>(A a, B b, C c, D d, E e, F f);

    /// <summary>
    /// 无参1返回值委托
    /// </summary>
    public delegate R EventFunction<R>();
    /// <summary>
    /// 1参1返回值委托
    /// </summary>
    public delegate R EventFunction<in A, R>(A a);
    /// <summary>
    /// 2参1返回值委托
    /// </summary>
    public delegate R EventFunction<in A, in B, R>(A a, B b);
    /// <summary>
    /// 3参1返回值委托
    /// </summary>
    public delegate R EventFunction<in A, in B, in C, R>(A a, B b, C c);

    /// <summary>
    /// 无参2返回值委托
    /// </summary>
    /// <typeparam name="R1"></typeparam>
    /// <typeparam name="R2"></typeparam>
    /// <returns></returns>
    public delegate (R1, R2) EventFunction2<R1, R2>();
    /// <summary>
    /// 1参2返回值委托
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="R1"></typeparam>
    /// <typeparam name="R2"></typeparam>
    /// <param name="a"></param>
    /// <returns></returns>
    public delegate (R1, R2) EventFunction2<in A, R1, R2>(A a);
    /// <summary>
    /// 2参2返回值委托
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <typeparam name="R1"></typeparam>
    /// <typeparam name="R2"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public delegate (R1, R2) EventFunction2<in A, in B, R1, R2>(A a, B b);
    #endregion
    #region 无参监听事件
    /// <summary>
    /// 无参数事件监听
    /// </summary>
    public static class EventManager
    {
        class EventData
        {
            public string key;
            public List<EventAction> actionList;
        }

        /// <summary>
        /// 带参数的EventManager事件监听清除
        /// </summary>
        public static List<EventAction> eventManagerClear { private set; get; }
        private static readonly List<EventData> eventList;
        //不在Clear内清理事件监听
        private static readonly List<EventData> eventList_dontClear;
        static EventManager()
        {
            eventManagerClear = new();
            eventList = new();
            eventList_dontClear = new();
        }
        /// <summary>
        /// 标记key，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="dontClear">是否在Clear()内自动批量清除监听</param>
        public static void AddListener(string key, EventAction action, bool dontClear)
        {
            bool isKey = false;
            if (dontClear)
            {
                eventList_dontClear.ForAction((eventData) =>
                {
                    if (eventData.key == key)
                    {
                        if (eventData.actionList.Contains(action))
                        { Debuger.LogError(key + "重复添加事件监听"); }
                        else
                        { eventData.actionList.Add(action); }
                        isKey = true;
                    }
                }, () => isKey);
                if (!isKey)
                {
                    eventList.Add(new EventData()
                    {
                        key = key,
                        actionList = new List<EventAction> { action }
                    });
                }
            }
            else
            {
                eventList.ForAction((eventData) =>
                {
                    if (eventData.key == key)
                    {
                        if (eventData.actionList.Contains(action))
                        { Debuger.LogError(key + "重复添加事件监听"); }
                        else
                        { eventData.actionList.Add(action); }
                        isKey = true;
                    }
                }, () => isKey);
                if (!isKey)
                {
                    eventList.Add(new EventData()
                    {
                        key = key,
                        actionList = new List<EventAction> { action }
                    });
                }
            }
        }
        /// <summary>
        /// 标记key，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener(string key, EventAction action)
        {
            AddListener(key, action, false);
        }
        /// <summary>
        /// 标记Enum，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<E>(E key, EventAction action) where E : Enum
        {
            AddListener($"{typeof(E).Name}.{key.ToString()}", action);
        }
        /// <summary>
        /// 标记Enum，添加一个事件监听
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="dontClear">是否在Clear()内自动批量清除监听</param>
        public static void AddListener<E>(E key, EventAction action, bool dontClear) where E : Enum
        {
            AddListener(key.ToString(), action, dontClear);
        }
        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveListener(string key)
        {
            bool isRemove = false;
            eventList.ForAction((eventData, index) =>
            {
                if (eventData.key == key)
                {
                    eventData.actionList = null;
                    eventList.RemoveAt(index);
                    isRemove = true;
                }
            }, () => isRemove
            );
            if (!isRemove)
            {
                eventList_dontClear.ForAction((eventData, index) =>
                {
                    if (eventData.key == key)
                    {
                        eventData.actionList = null;
                        eventList.RemoveAt(index);
                        isRemove = true;
                    }
                }, () => isRemove
            );
            }
            return isRemove;
        }
        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveListener<E>(E key) where E : Enum { return RemoveListener(key.ToString()); }
        /// <summary>
        /// 移除监听
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        [Obsolete("Use RemoveListener(key,action)!")]
        public static bool RemoveListener(EventAction action)
        {
            int removeCount = 0;
            eventList.ForAction((eventData, index) =>
            {
                if (eventData.actionList.Remove(action)) { removeCount++; }
            }
            );
            Debuger.LogWarning("移除监听个数：" + removeCount);
            return removeCount > 0;
        }
        /// <summary>
        /// 根据标记移除监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static bool RemoveListener(string key, EventAction action)
        {
            bool isRemove = false;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.actionList.Remove(action)) { isRemove = true; }
                }, () => isRemove
            );
            if (!isRemove) Debuger.LogWarning($"{key}中不包含指定的回调");
            return isRemove;
        }
        /// <summary>
        /// 根据标记移除监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void RemoveListener<E>(E key, EventAction action) where E : Enum
        {
            RemoveListener(key.ToString(), action);
        }
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="key"></param>
        public static void Broadcast(string key)
        {
            bool isKey = false;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.key == key)
                    {
                        eventData.actionList.ForAction(e => e?.Invoke());
                        isKey = true;
                    }
                }, () => isKey
            );
        }
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="key"></param>
        public static void Broadcast<E>(E key) where E : Enum { Broadcast(key.ToString()); }
        /// <summary>
        /// 清除所有委托
        /// </summary>
        public static void Clear()
        {
            eventList.Clear();
            for (int i = 0; i < eventManagerClear.Count; i++) eventManagerClear[i]?.Invoke();
        }
    }
    #endregion
    #region 1参数监听事件
    /// <summary>
    /// 1参数事件监听
    /// </summary>
    public static class EventManager<T>
    {
        class EventData
        {
            public string key;
            public List<EventAction<T>> actionList;
        }

        private static readonly List<EventData> eventList;
        //不在Clear内清理事件监听
        private static readonly List<EventData> eventList_dontClear;
        static EventManager()
        {
            eventList = new List<EventData>();
            //通过调用EventManager的静态成员，提前调用EventManager静态的构造方法
            EventManager.eventManagerClear.Add(() => EventManager<T>.Clear());
        }
        /// <summary>
        /// 标记key，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="dontClear">是否在Clear()内自动批量清除监听</param>
        public static void AddListener(string key, EventAction<T> action, bool dontClear)
        {
            bool isKey = false;
            if (dontClear)
            {
                eventList_dontClear.ForAction((eventData) =>
                {
                    if (eventData.key == key)
                    {
                        if (eventData.actionList.Contains(action))
                        { Debuger.LogError(key + "重复添加事件监听"); }
                        else
                        { eventData.actionList.Add(action); }
                        isKey = true;
                    }
                }, () => isKey);
                if (!isKey)
                {
                    eventList.Add(new EventData()
                    {
                        key = key,
                        actionList = new List<EventAction<T>> { action }
                    });
                }
            }
            else
            {
                eventList.ForAction((eventData) =>
                {
                    if (eventData.key == key)
                    {
                        if (eventData.actionList.Contains(action))
                        { Debuger.LogError(key + "重复添加事件监听"); }
                        else
                        { eventData.actionList.Add(action); }
                        isKey = true;
                    }
                }, () => isKey);
                if (!isKey)
                {
                    eventList.Add(new EventData()
                    {
                        key = key,
                        actionList = new List<EventAction<T>> { action }
                    });
                }
            }
        }
        /// <summary>
        /// 标记key，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener(string key, EventAction<T> action)
        {
            AddListener(key, action, false);
        }
        /// <summary>
        /// 标记Enum，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<E>(E key, EventAction<T> action) where E : Enum
        {
            AddListener($"{typeof(E).Name}.{key.ToString()}", action);
        }

        public static void AddListener<E>(E key, EventAction<T> action, bool dontClear) where E : Enum
        {
            AddListener($"{typeof(E).Name}.{key.ToString()}", action, dontClear);
        }
        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveListener(string key)
        {
            bool isRemove = false;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.key == key)
                    {
                        eventData.actionList = null;
                        eventList.RemoveAt(index);
                        isRemove = true;
                    }
                }, () => isRemove
            );
            return isRemove;
        }
        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveListener<E>(E key) where E : Enum { return RemoveListener(key.ToString()); }
        /// <summary>
        /// 移除监听
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        [Obsolete("Use RemoveListener(key,action)!")]
        public static bool RemoveListener(EventAction<T> action)
        {
            int removeCount = 0;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.actionList.Remove(action)) { removeCount++; }
                }
            );
            Debuger.LogWarning("移除监听个数：" + removeCount);
            return removeCount > 0;
        }
        /// <summary>
        /// 根据标记移除监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static bool RemoveListener(string key, EventAction<T> action)
        {
            bool isRemove = false;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.actionList.Remove(action)) { isRemove = true; }
                }, () => isRemove
            );
            if (!isRemove) Debuger.LogWarning($"{key}中不包含指定的回调");
            return isRemove;
        }
        /// <summary>
        /// 根据标记移除监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void RemoveListener<E>(E key, EventAction<T> action) where E : Enum
        {
            RemoveListener(key.ToString(), action);
        }
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="key"></param>
        /// <param name="t"></param>
        public static void Broadcast(string key, T t)
        {
            bool isKey = false;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.key == key)
                    {
                        eventData.actionList.ForAction(e => e?.Invoke(t));
                        isKey = true;
                    }
                }, () => isKey
            );
        }
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="key"></param>
        /// <param name="t"></param>
        public static void Broadcast<E>(E key, T t) where E : Enum { Broadcast(key.ToString(), t); }
        /// <summary>
        /// 清除所有委托
        /// </summary>
        public static void Clear()
        {
            Debuger.LogWarning($"clear <{typeof(T).Name}> all events!");
            eventList.Clear();
        }
    }
    #endregion
    #region 2参数监听事件
    /// <summary>
    /// 2参数事件监听
    /// </summary>
    public static class EventManager<T1, T2>
    {
        class EventData
        {
            public string key;
            public List<EventAction<T1, T2>> actionList;
        }

        private static readonly List<EventData> eventList;
        //不在Clear内清理事件监听
        private static readonly List<EventData> eventList_dontClear;
        static EventManager()
        {
            eventList = new List<EventData>();
            //通过调用EventManager的静态成员，提前调用EventManager静态的构造方法
            EventManager.eventManagerClear.Add(Clear);
        }
        /// <summary>
        /// 标记key，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="dontClear">是否在Clear()内自动批量清除监听</param>
        public static void AddListener(string key, EventAction<T1, T2> action, bool dontClear)
        {

            bool isKey = false;
            if (dontClear)
            {
                eventList_dontClear.ForAction((eventData) =>
                {
                    if (eventData.key == key)
                    {
                        if (eventData.actionList.Contains(action))
                        { Debuger.LogError(key + "重复添加事件监听"); }
                        else
                        { eventData.actionList.Add(action); }
                        isKey = true;
                    }
                }, () => isKey);
                if (!isKey)
                {
                    eventList.Add(new EventData()
                    {
                        key = key,
                        actionList = new List<EventAction<T1, T2>> { action }
                    });
                }
            }
            else
            {
                eventList.ForAction((eventData) =>
                {
                    if (eventData.key == key)
                    {
                        if (eventData.actionList.Contains(action))
                        { Debuger.LogError(key + "重复添加事件监听"); }
                        else
                        { eventData.actionList.Add(action); }
                        isKey = true;
                    }
                }, () => isKey);
                if (!isKey)
                {
                    eventList.Add(new EventData()
                    {
                        key = key,
                        actionList = new List<EventAction<T1, T2>> { action }
                    });
                }
            }
        }

        /// <summary>
        /// 标记key，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener(string key, EventAction<T1, T2> action)
        {
            AddListener(key, action, false);
        }
        /// <summary>
        /// 标记Enum，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<E>(E key, EventAction<T1, T2> action) where E : Enum
        {
            AddListener($"{typeof(E).Name}.{key.ToString()}", action);
        }
        /// <summary>
        /// 标记Enum，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="dontClear">是否在Clear()内自动批量清除监听</param>
        public static void AddListener<E>(E key, EventAction<T1, T2> action, bool dontClear) where E : Enum
        {
            AddListener($"{typeof(E).Name}.{key.ToString()}", action, dontClear);
        }
        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveListener(string key)
        {
            bool isRemove = false;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.key == key)
                    {
                        eventData.actionList = null;
                        eventList.RemoveAt(index);
                        isRemove = true;
                    }
                }, () => isRemove
            );
            return isRemove;
        }
        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveListener<E>(E key) where E : Enum { return RemoveListener(key.ToString()); }
        /// <summary>
        /// 移除监听
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        [Obsolete("Use RemoveListener(key,action)!")]
        public static bool RemoveListener(EventAction<T1, T2> action)
        {
            int removeCount = 0;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.actionList.Remove(action)) { removeCount++; }
                }
            );
            Debuger.LogWarning("移除监听个数：" + removeCount);
            return removeCount > 0;
        }
        /// <summary>
        /// 根据标记移除监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static bool RemoveListener(string key, EventAction<T1, T2> action)
        {
            bool isRemove = false;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.actionList.Remove(action)) { isRemove = true; }
                }, () => isRemove
            );
            if (!isRemove) Debuger.LogWarning($"{key}中不包含指定的回调");
            return isRemove;
        }
        /// <summary>
        /// 根据标记移除监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void RemoveListener<E>(E key, EventAction<T1, T2> action) where E : Enum
        {
            RemoveListener(key.ToString(), action);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public static void Broadcast(string key, T1 t1, T2 t2)
        {
            bool isKey = false;
            eventList.ForAction((eventData, index) =>
                {
                    if (eventData.key == key)
                    {
                        eventData.actionList.ForAction(e => e?.Invoke(t1, t2));
                        isKey = true;
                    }
                }, () => isKey
            );
        }
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="key"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public static void Broadcast<E>(E key, T1 t1, T2 t2) where E : Enum
        {
            Broadcast(key.ToString(), t1, t2);
        }
        /// <summary>
        /// 清除所有委托
        /// </summary>
        public static void Clear()
        {
            Debuger.LogWarning($"clear <{typeof(T1).Name}, {typeof(T2).Name}> all events!");
            eventList.Clear();
        }
    }
    #endregion
    #region 3参数监听事件
    /// <summary>
    /// 3参数事件监听
    /// </summary>
    public static class EventManager<T1, T2, T3>
    {
        class EventData
        {
            public string key;
            public List<EventAction<T1, T2, T3>> actionList;
        }

        private static readonly List<EventData> eventList;
        static EventManager()
        {
            eventList = new List<EventData>();
            //通过调用EventManager的静态成员，提前调用EventManager静态的构造方法
            EventManager.eventManagerClear.Add(Clear);
        }
        /// <summary>
        /// 标记key，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener(string key, EventAction<T1, T2, T3> action)
        {
            bool isKey = false;
            eventList.ForAction((eventData, index) =>
            {
                if (eventData.key == key)
                {
                    if (eventData.actionList.Contains(action)) { Debuger.LogError(key + "重复添加事件监听"); }
                    else { eventData.actionList.Add(action); }
                    isKey = true;
                }
            }, () => isKey
            );
            if (!isKey)
            {
                eventList.Add(new EventData() { key = key, actionList = new List<EventAction<T1, T2, T3>> { action } });
            }
        }
        /// <summary>
        /// 标记key，添加一个事件监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void AddListener<E>(E key, EventAction<T1, T2, T3> action) where E : System.Enum
        {
            AddListener(key.ToString(), action);
        }
        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveListener(string key)
        {
            bool isRemove = false;
            eventList.ForAction((eventData, index) =>
            {
                if (eventData.key == key)
                {
                    eventData.actionList = null;
                    eventList.RemoveAt(index);
                    isRemove = true;
                }
            }, () => isRemove
            );
            return isRemove;
        }
        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveListener<E>(E key) where E : System.Enum
        {
            return RemoveListener(key.ToString());
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        [Obsolete("Use RemoveListener(key,action)!")]
        public static bool RemoveListener(EventAction<T1, T2, T3> action)
        {
            int removeCount = 0;
            eventList.ForAction((eventData, index) =>
            {
                if (eventData.actionList.Remove(action)) { removeCount++; }
            }
            );
            Debuger.LogWarning("移除监听个数：" + removeCount);
            return removeCount > 0;
        }
        /// <summary>
        /// 根据标记移除监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static bool RemoveListener(string key, EventAction<T1, T2, T3> action)
        {
            bool isRemove = false;
            eventList.ForAction((eventData, index) =>
            {
                if (eventData.actionList.Remove(action)) { isRemove = true; }
            }, () => isRemove
            );
            if (!isRemove) Debuger.LogWarning($"{key}中不包含指定的回调");
            return isRemove;
        }
        /// <summary>
        /// 根据标记移除监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void RemoveListener<E>(E key, EventAction<T1, T2, T3> action) where E : System.Enum
        {
            RemoveListener(key.ToString(), action);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public static void Broadcast(string key, T1 t1, T2 t2, T3 t3)
        {
            bool isKey = false;
            eventList.ForAction((eventData, index) =>
            {
                if (eventData.key == key)
                {
                    eventData.actionList.ForAction(e => e?.Invoke(t1, t2, t3));
                    isKey = true;
                }
            }, () => isKey
            );
        }
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="key"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public static void Broadcast<E>(E key, T1 t1, T2 t2, T3 t3) where E : System.Enum
        {
            Broadcast(key.ToString(), t1, t2, t3);
        }
        /// <summary>
        /// 清除所有委托
        /// </summary>
        public static void Clear()
        {
            Debuger.LogWarning($"clear <{typeof(T1).Name}, {typeof(T2).Name}> all events!");
            eventList.Clear();
        }
    }
    #endregion
}