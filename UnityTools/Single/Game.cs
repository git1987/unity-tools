using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityTools.Single
{
    public sealed class Game : SingleMono<Game>
    {
        /// <summary>
        /// 延时调用的方法和计时器
        /// </summary>
        class DelayedData
        {
            public EventAction action;
            public float timer;
            public bool unscaledTime;
            //public DelayedData(EventAction ea, float time)
            //{
            //    this.action = ea;
            //    this.timer = time;
            //}
        }

        /// <summary>
        /// 清除所有延时调用方法
        /// </summary>
        public static void ClearDelayed()
        {
            GetInstance().delayedList.Clear();
        }
        /// <summary>
        /// 延时调用方法:time==0 时，隔一帧执行监听
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time">== 0 ，隔一帧执行监听</param>
        public static void Delayed(EventAction action, float time = 0, bool unscaledTime = false)
        {
            DelayedData ed = GetInstance().GetDelayedData(action);
            if (ed != null)
            {
                Debuger.LogError("已经存在回调了，添加了两次");
                return;
            }
            if (time <= 0)
                GetInstance().DelayedFrame(action);
            else
            {
                //新添加的回调，在当前帧就要执行Update，所以要提前加当前deltaTime补上
                ed = new()
                {
                    action = action,
                    unscaledTime = unscaledTime
                };
                if (unscaledTime)
                    ed.timer = time + Time.unscaledDeltaTime;
                else
                    ed.timer = time + Time.deltaTime;

                GetInstance().delayedList.Add(ed);
            }
        }
        /// <summary>
        /// 移除延时调用的方法
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool RemoveDelayed(EventAction action)
        {
            if (GetInstance().RemoveDelayedData(action))
            {
                return true;
            }
            Debuger.LogError("没有该延时调用的方法");
            return false;
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public static void Pause() { GetInstance().pause = true; }
        /// <summary>
        /// 继续
        /// </summary>
        public static void KeepOn() { GetInstance().pause = false; }
        /// <summary>
        /// 是否已经有了延时监听
        /// </summary>
        /// <returns></returns>
        public static bool HasAction(EventAction ea)
        {
            return instance.GetDelayedData(ea) != null;
        }
        private List<DelayedData> delayedList = new List<DelayedData>();
        //暂停
        private bool pause;
        private DelayedData GetDelayedData(EventAction ea)
        {
            for (int i = 0; i < delayedList.Count; i++)
            {
                if (delayedList[i].action == ea)
                {
                    return delayedList[i];
                }
            }
            return null;
        }
        private bool RemoveDelayedData(EventAction ea)
        {
            for (int i = 0; i < delayedList.Count; i++)
            {
                if (delayedList[i].action == ea)
                {
                    delayedList.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 延迟一帧执行监听
        /// </summary>
        /// <param name="action"></param>
        public void DelayedFrame(EventAction action)
        {
            StartCoroutine(_DelayedFrame(action));
        }
        IEnumerator _DelayedFrame(EventAction action)
        {
            yield return null;
            action();
        }
        private void Update()
        {
            if (pause) return;
            if (delayedList != null && delayedList.Count > 0)
            {
                for (int i = 0; i < delayedList.Count; i++)
                {
                    if (delayedList[i].unscaledTime)
                        delayedList[i].timer -= Time.unscaledDeltaTime;
                    else
                        delayedList[i].timer -= Time.deltaTime;
                    if (delayedList[i].timer <= 0)
                    {
                        delayedList[i].action.Invoke();
                        GetInstance().delayedList.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        protected override void OnDestroy()
        {
            delayedList.Clear();
            base.OnDestroy();
        }
    }
}