using UnityEngine;
using UnityTools.Single;
namespace UnityTools.MonoComponent
{
    /// <summary>
    /// 自动清除：删除/放回对象池
    /// </summary>
    public class AutoClear : MonoBehaviour
    {
        [SerializeField]
        private bool isEffect;
        /// 是否自动清除
        public bool autoClear => lifeTime > 0;
        [SerializeField]
        private float _lifeTime;
        public float lifeTime
        {
            private set => _lifeTime = value;
            get => _lifeTime;
        }
        [SerializeField]
        private GameObject deathObj;

        //特效播放完毕的回调
        private EventAction finish;
        private void Awake()
        {
            if (!isEffect) return;
            ParticleSystem[] pas = this.transform.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < pas.Length; i++)
            {
                ParticleSystem p = pas[i];
                lifeTime = Mathf.Max(lifeTime, p.main.duration);
            }
            Animator animator              = this.transform.GetComponentInChildren<Animator>();
            if (animator != null) lifeTime = Mathf.Max(lifeTime, animator.GetCurrentAnimatorStateInfo(0).length);
        }
        /// <summary>
        /// 设置特效播放完毕的回调
        /// </summary>
        /// <param name="_finish"></param>
        public void SetFinishAction(EventAction _finish)
        {
            if (autoClear)
                this.finish = _finish;
            else
                Debuger.LogError("不是自动清除的特效", this.gameObject);
        }

        //通过对象池使用，每次Get的时候调用
        private void OnEnable()
        {
            if (lifeTime <= 0) return;
            Schedule.GetInstance(this.gameObject).Once(this.Disable, lifeTime);
        }
        private void OnDisable()
        {
            if (deathObj == null || GameObjectPool.instance == null) return;
            GameObject effect = GameObjectPool.instance.Init(deathObj).GetObj(deathObj.name);
            Transform  tran   = this.transform;
            effect.transform.SetPositionAndRotation(tran.position, tran.rotation);
        }
        private void Disable()
        {
            this.finish?.Invoke();
            this.finish = null;
            GameObjectPool.Recover(this.gameObject);
        }
    }
}