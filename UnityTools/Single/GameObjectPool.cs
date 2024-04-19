using System.Collections.Generic;
using UnityEngine;
using UnityTools.Extend;

namespace UnityTools.Single
{
    /// <summary>
    /// GameObject对象池
    /// </summary>
    [AddComponentMenu("UnityTools/Single/GameObject对象池")]
    public class GameObjectPool : SingleMono<GameObjectPool>
    {
        public enum EventType
        {
            /// <summary>
            /// 初始化GameObjectPool
            /// </summary>
            Init,
            /// <summary>
            /// 初始化一个对象：string[prefab name]
            /// </summary>
            InitObj,
            /// <summary>
            /// 清理一个对象：string[prefab name]
            /// </summary>
            ClearObj,
            /// <summary>
            /// 移除一个对象：string[prfab name]
            /// </summary>
            RemoveObj
        }
        public readonly string EventKey = "Event_GameObjectPool";
        /// <summary>
        /// 回收GameObject对象，如果没有创建Pool则被Destroy掉
        /// </summary>
        /// <param name="go"></param>
        /// <param name="resetTransform">是否重置transform</param>
        public static void Recover(GameObject go, bool resetTransform = false)
        {
            if (go == null) return;
            if (instance == null)
                Destroy(go);
            else
                instance.RecoverObj(go, resetTransform);
        }
        /// <summary>
        /// 获取一个GameObject对象
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <returns></returns>
        public static GameObject Get(string gameObjectName)
        {
            if (instance == null)
            {
                Debuger.LogError("There is no Pool component in the scene");
            }
            return instance.GetObj(gameObjectName);
        }
        /// <summary>
        /// 移除GameObject对象
        /// </summary>
        /// <param name="gameObjectName"></param>
        public static void Remove(string gameObjectName)
        {
            if (instance == null)
                Debuger.Log(Tools.SetTextColor("There is no Pool component in the scene", Config.RichTextColor.Red));
            else
                instance.RemoveObj(gameObjectName);
        }
        private readonly Dictionary<string, GameObject> poolPrefab = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Stack<GameObject>> pools = new Dictionary<string, Stack<GameObject>>();
        private readonly Dictionary<string, int> poolCount = new Dictionary<string, int>();
        private Transform poolParent;
        private Transform useParent;
#if UNITY_EDITOR
        /// <summary>
        /// 便于编辑器内查看
        /// </summary>
        [SerializeField]
        private List<GameObject> tempList = new List<GameObject>();
#endif
        /// <summary>
        /// GetObj()
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <returns></returns>
        public GameObject this[string gameObjectName]
        {
            get => this.GetObj(gameObjectName);
            set => RecoverObj(value, false);
        }
        protected override void Awake()
        {
            if (_instance == null)
            {
                EventManager.Broadcast(EventType.Init);
                useParent = new GameObject("useParent").transform;
                useParent.SetParentReset(this.transform);
                poolParent = new GameObject("objParent").transform;
                poolParent.SetParentReset(this.transform);
            }
            base.Awake();
        }
        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="prefab">Prefab</param>
        /// <param name="count">初始化数量</param>
        /// <param name="reset">重置transform</param>
        /// <returns>Pool</returns>
        public GameObjectPool Init(GameObject prefab, int count = 0, bool reset = false)
        {
            if (prefab == null) { Debuger.LogError("the prefab is null"); }
            else
            {
                if (poolPrefab.ContainsKey(prefab.name))
                {
                    Debuger.LogWarning($"[{prefab.name}] already exists", this.gameObject);
                }
                else
                {
                    Stack<GameObject> goQueue = new Stack<GameObject>();
                    for (int i = 0; i < count; i++)
                    {
                        GameObject go = Instantiate(prefab);
                        Transform tran = go.transform;
                        if (reset)
                        {
                            tran.localPosition = Vector3.zero;
                            if (tran is RectTransform)
                            {
                                (tran as RectTransform).anchoredPosition3D = Vector3.zero;
                            }
                            tran.localRotation = Quaternion.identity;
                            tran.localScale = Vector3.one;
                        }
                        tran.SetParent(poolParent);
                        go.SetActive(false);
                        goQueue.Push(go);
                    }
                    pools.Add(prefab.name, goQueue);
                    poolPrefab.Add(prefab.name, prefab);
                    EventManager<string>.Broadcast(EventType.InitObj, prefab.name);
#if UNITY_EDITOR
                    tempList.Add(prefab);
#endif
                }
            }
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <returns></returns>
        public bool Has(string gameObjectName)
        {
            return poolPrefab.ContainsKey(gameObjectName);
        }
        public bool Has(string gameObjectName, out GameObject obj)
        {
            if (Has(gameObjectName))
            {
                obj = GetObj(gameObjectName);
                return true;
            }
            else
            {
                obj = null;
                return false;
            }
        }
        /// <summary>
        /// 设置对象容量
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <param name="count"></param>
        public GameObjectPool SetResize(string gameObjectName, int count)
        {
            if (pools.TryGetValue(gameObjectName, out Stack<GameObject> queue))
            {
                if (poolCount.ContainsKey(gameObjectName))
                {
                    poolCount[gameObjectName] = Mathf.Max(count, queue.Count);
                }
                else { poolCount.Add(gameObjectName, Mathf.Max(count, queue.Count)); }
            }
            else { Debuger.LogError($"[{gameObjectName}] does not exist"); }
            return this;
        }
        /// <summary>
        /// 库存：对象池中当前对象的个数
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <returns></returns>
        private int Stock(string gameObjectName)
        {
            if (poolCount.TryGetValue(gameObjectName, out int count)) { return count; }
            return int.MaxValue;
        }
        /// <summary>
        /// 对象出纳：+1入库，-1出库
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <param name="cashier"></param>
        private void Transfer(string gameObjectName, short cashier)
        {
            if (poolCount.TryGetValue(gameObjectName, out int count))
            {
                poolCount[gameObjectName] = Mathf.Max(count + cashier, 0);
                Debuger.LogWarning($"[{gameObjectName}]remaining number：{poolCount[gameObjectName]}");
            }
        }
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <returns></returns>
        public GameObject GetObj(string gameObjectName)
        {
            if (gameObjectName is { Length: < 1 })
            {
                Debuger.LogError("name is empty!");
                return null;
            }
            GameObject temp = null;
            if (poolPrefab.TryGetValue(gameObjectName, out GameObject go))
            {
                if (Stock(gameObjectName) > 0)
                {
                    if (pools[gameObjectName].Count > 0)
                    {
                        temp = pools[gameObjectName].Pop();
                    }
                    else
                    {
                        temp = Instantiate(go);
                        temp.name = gameObjectName;
                    }
                    temp.transform.SetParent(useParent);
                    temp.SetActive(true);
                    Transfer(gameObjectName, -1);
                }
                else { Debuger.LogWarning("not enough " + gameObjectName); }
            }
            else { Debuger.LogError($"[{gameObjectName}] does not exist"); }
            return temp;
        }
        /// <summary>
        /// 回收对象：如果不属于对象池的GameObject则被Destroy
        /// <para>resetTransform:</para>
        /// <para>localPosition = Vector3.zero</para>
        /// <para>localRotation = Quaternion.identity[Vector3.zero]</para>
        /// <para>localScale = Vector3.one</para>
        /// </summary>
        /// <param name="go"></param>
        /// <param name="resetTransform">重置transform</param>
        private void RecoverObj(GameObject go, bool resetTransform)
        {
            if (go == null) return;
            if (poolPrefab.ContainsKey(go.name))
            {
                if (pools[go.name].Contains(go))
                {
                    Debuger.LogError($"[{go.name}] has already been placed in the Pool！", go);
                    return;
                }
                go.transform.SetParent(poolParent);
                if (resetTransform)
                {
                    Transform tran = go.transform;
                    tran.localPosition = Vector3.zero;
                    tran.localRotation = Quaternion.identity;
                    tran.localScale = Vector3.one;
                }
                go.SetActive(false);
                pools[go.name].Push(go);
                Transfer(name, 1);
            }
            else
            {
                Debug.LogWarning($"[{go.name}] is not in Pool");
                Destroy(go);
            }
        }
        /// <summary>
        /// 清理对象：保留对象prefab，清除已生成的对象
        /// </summary>
        public void ClearObj(string gameObjectName)
        {
            if (poolPrefab.ContainsKey(gameObjectName))
            {
                foreach (GameObject go in pools[gameObjectName]) { Destroy(go); }

                pools[gameObjectName].Clear();
                EventManager<string>.Broadcast(EventType.ClearObj, gameObjectName);
                Debuger.LogWarning($"clear [{gameObjectName}]");
            }
            else { Debuger.LogWarning($"[{gameObjectName}] does not exist"); }
        }
        /// <summary>
        /// 移除对象
        /// </summary>
        /// <param name="gameObjectName"></param>
        public void RemoveObj(string gameObjectName)
        {
            if (poolPrefab.ContainsKey(gameObjectName))
            {
                foreach (GameObject go in pools[gameObjectName]) { Destroy(go); }
#if UNITY_EDITOR
                tempList.Remove(poolPrefab[gameObjectName]);
#endif
                pools[gameObjectName].Clear();
                pools.Remove(gameObjectName);
                poolPrefab.Remove(gameObjectName);
                EventManager<string>.Broadcast(EventType.RemoveObj, gameObjectName);
                Debuger.LogWarning($"remove [{gameObjectName}]");
            }
            else { Debuger.LogWarning($"[{gameObjectName}] does not exist"); }
        }
    }
}