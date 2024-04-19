using UnityEngine;

namespace UnityTools.MonoComponent
{
    /// <summary>
    /// 计时任务
    /// </summary>
    public class Schedule : MonoBehaviour
    {
        /// <summary>
        /// 计时数据
        /// </summary>
        public struct ScheduleData
        {
            /// <summary>
            /// 普通回调
            /// </summary>
            public EventAction action;

            /// <summary>
            /// 重复回调：当前的次数（index）
            /// </summary>
            public EventAction<int> repeatedAction;

            /// <summary>
            /// 任务结束后的回调
            /// </summary>
            public EventAction finish;

            /// <summary>
            /// 持续的最长时间
            /// </summary>
            public float maxTime;

            /// <summary>
            /// 周期
            /// </summary>
            public float periodTime;

            /// <summary>
            /// 重复次数
            /// </summary>
            public int repeat;
        }

        public static Schedule GetInstance(GameObject go)
        {
            Schedule schedule = go.AddComponent<Schedule>();
            return schedule;
        }

        //计时任务是否结束
        bool over;
        //开关
        bool enable;
        //计时器
        float timer;
        //当前循环调用的方法次数
        int repeatIndex;
        ScheduleData scheduleData;
        //是否受timeScale影响
        bool unscaleTime;
        /// <summary>
        /// 延迟调用一次
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time">等待时间</param>
        public Schedule Once(EventAction action, float time, bool unscaleTime = false)
        {
            if (time <= 0)
            {
                Debuger.Log("time<=0？");
                action?.Invoke();
                Stop(true);
                return this;
            }
            if (action == null)
            {
                Debuger.LogError("callback is null！");
                return this;
            }
            scheduleData = new ScheduleData() { action = action, maxTime = float.MaxValue };
            enable = true;
            this.unscaleTime = unscaleTime;
            if (unscaleTime)
                timer = time + Time.unscaledDeltaTime;
            else
                timer = time + Time.deltaTime;
            return this;
        }
        /// <summary>
        /// 重复调用方法
        /// </summary>
        /// <param name="repeatedAction"></param>
        /// <param name="startTime"></param>
        /// <param name="periodTime"></param>
        /// <param name="repeat"></param>
        /// <param name="maxTime"></param>
        /// <param name="finish"></param>
        public Schedule Repeated(EventAction<int> repeatedAction, float startTime, float periodTime, int repeat,
                             float maxTime, EventAction finish, bool unscaleTime = false)
        {
            enable = true;
            scheduleData = new ScheduleData()
            {
                repeatedAction = repeatedAction,
                periodTime = periodTime,
                repeat = repeat,
                maxTime = maxTime,
                finish = finish
            };
            if (unscaleTime)
                timer = startTime + Time.unscaledDeltaTime;
            else
                timer = startTime + Time.deltaTime;
            if (scheduleData.maxTime < float.MaxValue)
            {
                if (unscaleTime)
                    scheduleData.maxTime += Time.unscaledDeltaTime;
                else
                    scheduleData.maxTime += Time.deltaTime;
            }
            repeatIndex = 0;
            this.unscaleTime = unscaleTime;
            return this;
        }
        /// <summary>
        /// 暂停计时任务
        /// </summary>
        public Schedule Pause()
        {
            enable = false;
            return this;
        }
        /// <summary>
        /// 继续计时任务
        /// </summary>
        public Schedule KeepOn()
        {
            enable = true;
            return this;
        }
        /// <summary>
        /// 停止计时任务
        /// </summary>
        /// <param name="isComplete">是否执行完成计时任务回调</param>
        public void Stop(bool isComplete)
        {
            over = true;
            if (isComplete) { scheduleData.finish?.Invoke(); }
            Destroy(this);
        }
        private void Update()
        {
            if (!enable) return;
            switch (timer)
            {
                case <= 0:
                    scheduleData.action?.Invoke();
                    scheduleData.repeatedAction?.Invoke(repeatIndex++);
                    if (scheduleData.repeat < int.MaxValue) scheduleData.repeat--;
                    if (scheduleData.repeat <= 0)
                    {
                        //次数用完了
                        Stop(true);
                    }
                    else { timer += scheduleData.periodTime; }
                    break;
                case < float.MaxValue:
                    if (unscaleTime)
                        timer -= Time.unscaledDeltaTime;
                    else
                        timer -= Time.deltaTime;
                    break;
            }
            if (scheduleData.maxTime < float.MaxValue)
            {
                if (unscaleTime)
                    scheduleData.maxTime -= Time.unscaledDeltaTime;
                else
                    scheduleData.maxTime -= Time.deltaTime;
                if (scheduleData.maxTime <= 0)
                {
                    //时间到了
                    Stop(true);
                    return;
                }
            }
        }
        private void OnDisable()
        {
            //如果计时器没有结束，被隐藏了，自动调用Stop，并且不触发回调
            if (!over) Stop(false);
        }
    }
}