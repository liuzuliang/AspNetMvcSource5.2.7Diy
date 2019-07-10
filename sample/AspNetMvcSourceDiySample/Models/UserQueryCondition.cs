using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Models
{
    /// <summary>
    /// 备注：属性 HobbyList 将通过动态注册。
    /// </summary>
    public class UserQueryCondition
    {
        public int[] AgesIn { get; set; }

        public ICollection<int> HobbyList { get; set; }

        public List<DateTime> Date_Of_Birth_In { get; set; }
    }
}