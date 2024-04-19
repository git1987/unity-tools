using UnityEngine;
using UnityEngine.UI;
using UnityTools.Extend;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UnityTools.UI
{
    /// <summary>
    /// 虚拟摇杆
    /// </summary>
    [AddComponentMenu("UnityTools/UI/虚拟摇杆")]
    public class VirtualRocker : MonoBehaviour
    {
#if UNITY_EDITOR
        [CustomEditor(typeof(VirtualRocker))]
        public class VirtualRockerEditor : Editor
        {
            private Vector2 direction;
            private void OnEnable() { }
            private void OnSceneGUI()
            {
                VirtualRocker vr = target as VirtualRocker;
                direction = vr.Direction;
            }
            public override void OnInspectorGUI()
            {
                if (Application.isPlaying)
                {
                    direction = EditorGUILayout.Vector2Field("当前方向", direction);
                    VirtualRocker virtualRocker = target as VirtualRocker;
                    virtualRocker.unenableHide =
                        EditorGUILayout.Toggle("未激活时隐藏摇杆", virtualRocker.unenableHide);
                    if (!virtualRocker.unenableHide)
                    {
                        virtualRocker.restPos = EditorGUILayout.Toggle("未激活时是否回到原位", virtualRocker.restPos);
                    }
                    virtualRocker.showGUI = EditorGUILayout.Toggle("显示Log信息", virtualRocker.showGUI);
                    return;
                }
                // base.OnInspectorGUI();
                VirtualRocker vr = target as VirtualRocker;
                using (new EditorGUI.DisabledScope(true))
                {
                    if (vr.canvasRect == null)
                    {
                        Canvas c = vr.GetComponentInParent<Canvas>();
                        if (c == null)
                        {
                            EditorGUILayout.HelpBox("进入编辑模式或者实例化在场景中，并设置父级为Canvas",
                                MessageType.Warning
                            );
                            return;
                        }
                        vr.canvasRect = c.transform as RectTransform;
                    }
                    if (vr._point == null || vr._pointer == null || vr._pointBg == null || vr.areaRect == null)
                    {
                        EditorGUILayout.HelpBox("子级丢失，请重新创建", MessageType.Error);
                        return;
                    }
                    if (vr.canvasRect == null)
                    {
                        EditorGUILayout.HelpBox("放在Canvas子级里", MessageType.Error);
                        return;
                    }
                    EditorGUILayout.ObjectField("Canvas", vr.canvasRect, typeof(RectTransform), true);
                    if (vr.gp != null)
                    {
                        EditorGUILayout.ObjectField("GraphicPointer", vr.gp, typeof(GraphicPointer), true);
                    }
                    if (vr.areaRect == null)
                        GUILayout.Label("虚拟摇杆触发区域丢失，请重新创建");
                    else
                        EditorGUILayout.ObjectField("虚拟摇杆触发区域", vr.areaRect, typeof(RectTransform), true);
                    EditorGUILayout.ObjectField("虚拟摇杆背景", vr._pointBg, typeof(RectTransform), true);
                    EditorGUILayout.ObjectField("虚拟摇杆点", vr._point, typeof(RectTransform), true);
                    EditorGUILayout.ObjectField("虚拟摇杆指针", vr._pointer, typeof(RectTransform), true);
                }
                GUILayout.Space(5);
                vr.unenableHide = EditorGUILayout.Toggle("未激活时隐藏摇杆", vr.unenableHide);
                if (!vr.unenableHide)
                {
                    vr.restPos = EditorGUILayout.Toggle("未激活时是否回到原位", vr.restPos);
                    vr.backCenterPos = EditorGUILayout.Toggle("是否回到区域中心位置", vr.backCenterPos);
                    if (!vr.backCenterPos)
                    {
                        vr.oldPos = EditorGUILayout.Vector2Field("要回到的初始位置", vr.oldPos);
                    }
                }
                if (vr) GUILayout.Space(5);
                vr.ignoreUI = EditorGUILayout.Toggle("是否忽略UI遮挡", vr.ignoreUI);
                vr._pointBg.gameObject.SetActive(!vr.unenableHide);
                RectTransform area = vr.transform.Find("area") as RectTransform;
                if (area == null) throw new System.NullReferenceException("area is null!");
                area.GetComponent<Image>().raycastTarget = !vr.ignoreUI;
                if (!vr.ignoreUI)
                {
                    vr.gp = area.gameObject.MateComponent<GraphicPointer>();
                }
                else
                {
                    if (vr.gp != null) DestroyImmediate(vr.gp);
                    vr.gp = null;
                }
                if (GUILayout.Button("保存"))
                {
                    EditorUtility.SetDirty(vr.gameObject);
                    AssetDatabase.SaveAssetIfDirty(vr.gameObject);
                }
            }
        }

        /// <summary>
        /// 选择CanvasGameObject创建一个虚拟摇杆
        /// </summary>
        /// <param name="canvasObj"></param>
        /// <returns></returns>
        public static GameObject CreateGameObject(GameObject canvasObj)
        {
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debuger.LogError("请选择带有Canvas组件的GameObject");
                return null;
            }
            Object[] objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra");
            System.Collections.Generic.Dictionary<string, Sprite> dics =
                new System.Collections.Generic.Dictionary<string, Sprite>();
            foreach (var obj in objs)
            {
                if (obj is Sprite)
                {
                    dics.Add(obj.name, obj as Sprite);
                }
            }
            RectTransform canvasRect = canvas.transform as RectTransform;
            GameObject rocker = new GameObject("VirtualRocker");
            VirtualRocker virtualRocker = rocker.AddComponent<VirtualRocker>();
            virtualRocker.canvasRect = canvasRect;
            rocker.transform.SetParentReset(canvasRect);
            RectTransform rect = rocker.AddComponent<RectTransform>();
            Tools.RectTransformSetSurround(rect);
            GameObject bg = new GameObject("pointBg");
            rect = bg.AddComponent<RectTransform>();
            virtualRocker._pointBg = rect;
            rect.SetParentReset(rocker.transform);
            rect.sizeDelta = Vector2.one * 200;
            rect.anchoredPosition = new Vector2(canvasRect.sizeDelta.x / 2f, canvasRect.sizeDelta.y / 2f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            bg.MateComponent<Image>().raycastTarget = false;
            bg.MateComponent<Image>().color = new Color(1, 1, 1, .5f);
            bg.MateComponent<Image>().sprite = dics["Knob"];
            GameObject pointer = new GameObject("pointer");
            rect = pointer.AddComponent<RectTransform>();
            virtualRocker._pointer = rect;
            rect.SetParentReset(bg.transform);
            rect.sizeDelta = new Vector2(10, 100);
            rect.anchoredPosition = Vector2.zero;
            rect.pivot = new Vector2(.5f, 0);
            pointer.AddComponent<Image>().raycastTarget = false;
            GameObject point = new GameObject("point");
            rect = point.AddComponent<RectTransform>();
            virtualRocker._point = rect;
            rect.SetParentReset(bg.transform);
            rect.sizeDelta = Vector2.one * 50;
            rect.anchoredPosition = Vector2.zero;
            point.MateComponent<Image>().raycastTarget = false;
            point.MateComponent<Image>().color = Color.red;
            point.MateComponent<Image>().sprite = dics["Knob"];
            GameObject area = new GameObject("area");
            rect = area.AddComponent<RectTransform>();
            virtualRocker.areaRect = rect;
            rect.SetParentReset(rocker.transform);
            rect.SetAsFirstSibling();
            rect.sizeDelta = Vector2.one * 500;
            Image areaImage = area.AddComponent<Image>();
            areaImage.color = Vector4.one * .5f;
            areaImage.raycastTarget = false;
            return rocker;
        }
        bool showGUI;
        private void OnGUI()
        {
            if (isClick && showGUI)
            {
                EditorGUILayout.Vector2Field("方向", Direction);
            }
        }
#endif
        public bool isClick { private set; get; }
        [SerializeField]
        private RectTransform canvasRect;
        /// <summary>
        /// 未激活时是否隐藏
        /// </summary>
        [SerializeField]
        private bool unenableHide;
        /// <summary>
        /// 是否忽略UI遮挡
        /// </summary>
        [SerializeField]
        private bool ignoreUI;
        /// <summary>
        /// 未激活时是否回到初始位置
        /// </summary>
        private bool restPos;
        [SerializeField]
        private bool backCenterPos;
        public Vector2 oldPos;
        [SerializeField]
        private GraphicPointer gp;
        /// <summary>
        /// 摇杆区域背景
        /// </summary>
        [SerializeField]
        private RectTransform _pointBg;
        public RectTransform pointBg => _pointBg;
        /// <summary>
        /// 摇杆点
        /// </summary>
        [SerializeField]
        private RectTransform _point;
        public RectTransform point => _point;
        //是否显示虚拟摇杆点
        private bool showPoint;
        /// <summary>
        /// 摇杆方向指针
        /// </summary>
        [SerializeField]
        private RectTransform _pointer;
        public RectTransform pointer => _pointer;
        //是否显示虚拟摇杆指针
        private bool showPointer;
        /// <summary>
        /// 可触发摇杆的区域
        /// </summary>
        [SerializeField]
        private RectTransform areaRect;
        private Vector2 clickMousePos, currentMousePos;
        /// <summary>
        /// 虚拟摇杆当前的Vector2向量
        /// </summary>
        public Vector2 Direction => _point.anchoredPosition / ((_pointBg.sizeDelta - _point.sizeDelta) / 2);
        protected virtual void Awake()
        {
            CanvasScaler cs = this.GetComponentInParent<CanvasScaler>();
            if (gp != null)
            {
                gp.SetDownAction(() =>
                                 {
                                     if (canvasRect == null)
                                     {
                                         Debuger.LogError("没有设置canvas[SetCanvas()]");
                                         return;
                                     }
                                     else
                                     {
                                         clickMousePos = Config.screenPosition;
                                         SetPoint();
                                     }
                                 }
                                );
                gp.SetUpAction(ResetRocker);
            }
            showPoint = _point.GetComponent<Image>().sprite != null;
            _point.gameObject.SetActive(showPoint);
            showPointer = _pointer.GetComponent<Image>().sprite != null;
            _pointer.gameObject.SetActive(showPointer);
            ResetRocker();
        }
        /// <summary>
        /// 开启虚拟摇杆
        /// </summary>
        public void Enable()
        {
            gameObject.SetActive(true);
        }
        /// <summary>
        /// 禁用虚拟摇杆
        /// </summary>
        public void Unenable()
        {
            Stop();
            gameObject.SetActive(false);
        }
        /// <summary>
        /// 停止虚拟摇杆
        /// </summary>
        public void Stop()
        {
            ResetRocker();
        }
        public void SetCanvas(RectTransform canvas)
        {
            canvasRect = canvas;
        }

        /// <summary>
        /// 设置虚拟摇杆状态
        /// </summary>
        /// <param name="unenableHide"></param>
        /// <param name="_restPos"></param>
        /// <param name="backCenterPos"></param>
        /// <param name="backPos"></param>
        public void SetState(bool _unenableHide, bool _restPos, bool _backCenterPos, Vector2 _backPos)
        {
            unenableHide = _unenableHide;
            backCenterPos = _backCenterPos;
            oldPos = _backPos;
            ResetRocker();
        }
        /// <summary>
        /// 是否触发虚拟摇杆UI
        /// </summary>
        protected virtual void CheckShowRocker()
        {
            if (canvasRect == null) throw new System.Exception("没有设置canvas[SetCanvas()]");
            //area的边界
            float minX = areaRect.anchorMin.x * canvasRect.sizeDelta.x + areaRect.offsetMin.x,
                  maxX = areaRect.anchorMax.x * canvasRect.sizeDelta.x + areaRect.offsetMax.x,
                  minY = areaRect.anchorMin.y * canvasRect.sizeDelta.y + areaRect.offsetMin.y,
                  maxY = areaRect.anchorMax.y * canvasRect.sizeDelta.y + areaRect.offsetMax.y;
            clickMousePos = Config.screenPosition;
            Vector2 canvasPos = clickMousePos * canvasRect.sizeDelta / new Vector2(Screen.width, Screen.height);
            if (canvasPos.x >= minX && canvasPos.x <= maxX && canvasPos.y >= minY && canvasPos.y <= maxY)
            {
                SetPoint();
            }
        }
        void SetPoint()
        {
            isClick = true;
            _pointBg.gameObject.SetActive(true);
            _pointBg.anchoredPosition = clickMousePos * canvasRect.sizeDelta / new Vector2(Screen.width, Screen.height);
        }
        /// <summary>
        /// 重置虚拟摇杆UI
        /// </summary>
        protected virtual void ResetRocker()
        {
            if (canvasRect == null)
            {
                Debuger.LogError("没有设置canvas[SetCanvas()]");
                return;
            }
            isClick = false;
            currentMousePos = Vector2.negativeInfinity;
            if (unenableHide)
            {
                _pointBg.gameObject.SetActive(false);
            }
            else
            {
                if (restPos)
                {
                    //area的边界
                    float minX = areaRect.anchorMin.x * canvasRect.sizeDelta.x + areaRect.offsetMin.x,
                          maxX = areaRect.anchorMax.x * canvasRect.sizeDelta.x + areaRect.offsetMax.x,
                          minY = areaRect.anchorMin.y * canvasRect.sizeDelta.y + areaRect.offsetMin.y,
                          maxY = areaRect.anchorMax.y * canvasRect.sizeDelta.y + areaRect.offsetMax.y;
                    if (backCenterPos)
                        _pointBg.anchoredPosition = new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
                    else
                        _pointBg.anchoredPosition = oldPos;
                }
                _point.anchoredPosition = Vector2.zero;
                _pointBg.gameObject.SetActive(true);
                _pointer.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 更新虚拟摇杆UI
        /// </summary>
        protected virtual void UpdateRocker()
        {
            if (!isClick) return;
            if (currentMousePos == Config.screenPosition) return;
            currentMousePos = Config.screenPosition;
            /*设置point位置点*/
            //if (showPoint)
            {
                _point.anchoredPosition = currentMousePos - clickMousePos;
                if (_point.anchoredPosition.magnitude > _pointBg.sizeDelta.x / 2 - _point.sizeDelta.x / 2)
                    _point.anchoredPosition = _point.anchoredPosition.normalized *
                                              (_pointBg.sizeDelta.x / 2 - _point.sizeDelta.x / 2);
            }
            /*设置pointer的方向*/
            if (showPointer)
            {
                _pointer.gameObject.SetActive(true);
                float angle = Vector2.SignedAngle(Vector2.up, Direction);
                _pointer.eulerAngles = new Vector3(0, 0, angle);
                _pointer.gameObject.SetActive(Direction != Vector2.zero);
            }
        }
        protected virtual void Update()
        {
            if (gp == null)
            {
                if (Config.leftMouseDown)
                {
                    CheckShowRocker();
                }
                else if (Config.leftMouseUp)
                {
                    ResetRocker();
                }
            }
            UpdateRocker();
        }
    }
}