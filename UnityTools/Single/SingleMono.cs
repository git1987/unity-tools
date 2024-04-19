using UnityEngine;

namespace UnityTools.Single
{
    /// <summary>
    /// MonoBehaviour单列基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingleMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        protected static T _instance = null;

        /// <summary>
        /// 单例类对象：需要自行判断是否为空，若想使用不为空的单例，使用GetInstance()
        /// </summary>
        public static T instance
        {
            get
            {
                if (_instance == null) Debuger.LogError(typeof(T).Name + " is not create!");
                return _instance;
            }
        }
        /// <summary>
        /// 获取单例类对象，若没有则创建一个
        /// </summary>
        /// <returns></returns>
        public static T GetInstance(GameObject componentGameObject = null)
        {
            if (_instance == null)
            {
                if (Application.isPlaying)
                {
                    if (componentGameObject == null) componentGameObject = new GameObject(typeof(T).Name);
                    Debuger.LogWarning($"create {typeof(T).Name}!", componentGameObject);
                    _instance = componentGameObject.AddComponent<T>();
                }
            }
            return _instance;
        }
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                T t = this.GetComponent<T>();
                _instance = t;
            }
            else
            {
                if (_instance != this)
                {
                    Debuger.LogError($"The [{this.GetType().Name}] already exists", this.gameObject);
                    Destroy(this);
                }
            }
        }
        protected virtual void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}