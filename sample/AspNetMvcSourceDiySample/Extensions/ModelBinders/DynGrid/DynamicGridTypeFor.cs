using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    /// <summary>
    /// 动态
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicGridTypeFor<T>
        where T : class, new()
    {
        /// <summary>
        /// 动态注册
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propExpression"></param>
        /// <param name="dgConfig"></param>
        public DynamicGridTypeFor<T> Register<TProperty>(Expression<Func<T, TProperty>> propExpression, DynamicGridConfig dgConfig)
            where TProperty : IEnumerable
        {
            if (propExpression == null)
            {
                throw new ArgumentNullException(nameof(propExpression));
            }
            if (dgConfig == null)
            {
                throw new ArgumentNullException(nameof(dgConfig));
            }
            DynamicGridTypeContainer.Register(propExpression, dgConfig);
            return this;
        }
    }
}