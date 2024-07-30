using LIM.EntityServices.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LIM.Helpers
{
    public static class ReflectionHelper
    {
        public static string GetMsListColumnName<T>(System.Linq.Expressions.Expression<Func<T, object>> expression)
        {
            var member = expression.Body as System.Linq.Expressions.MemberExpression ??
                         ((System.Linq.Expressions.UnaryExpression)expression.Body).Operand as System.Linq.Expressions.MemberExpression;

            if (member == null)
            {
                throw new ArgumentException("Expression is not a member access", nameof(expression));
            }

            var attribute = member.Member.GetCustomAttribute<MsListColumn>();

            if (attribute == null)
            {
                throw new ArgumentException("The specified property does not have a MsListColumn attribute", nameof(expression));
            }

            return attribute.Name;
        }
    }
}
