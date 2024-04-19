using System.Collections.Generic;
using UnityTools.Extend;
namespace UnityTools.UI
{
    public abstract class BaseModel
    {
        private static List<BaseModel> modelList = new();
        protected static void CreateModel(BaseModel model)
        {
            modelList.Add(model);
        }
        /// <summary>
        /// 清空所有的Model单例
        /// </summary>
        public static void ClearModel()
        {
            modelList.ForAction(model => model.Disable());
        }
        /// <summary>
        /// 移除：清空单例
        /// </summary>
        protected abstract void Disable();
    }
}