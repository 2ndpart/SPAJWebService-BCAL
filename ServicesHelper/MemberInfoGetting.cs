using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;

namespace HtmlGeneratorServices.ServicesHelper
{
    public static class MemberInfoGetting
    {
        public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        {
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            return expressionBody.Member.Name;
        }
    }
}