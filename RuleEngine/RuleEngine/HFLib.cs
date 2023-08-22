using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RuleEngine
{
    internal static class HFLib
    {
        public class ContextParameter
        {
            public Type ParameterType { get; set; }
            public string Name { get; set; }
        }
        public static Expression ParseExpressionStrToTree(string exp, List<ContextParameter> parameters)
        {
            ParameterExpression[] parameterArray = new ParameterExpression[parameters.Count];
            int i = 0;
            //List<Expression> ps = new List<Expression>();
            foreach (ContextParameter p in parameters)
            {
                parameterArray[i] = Expression.Parameter(p.ParameterType, p.Name);
            }
            return System.Linq.Dynamic.DynamicExpression.ParseLambda(parameterArray, null, exp);

        }

        public static bool ExecuteOn(Expression parseTree, TargetContext record)
        {
            return (bool)((LambdaExpression)parseTree).Compile().DynamicInvoke(record);
        }

        public static void PrintPreOrder(Expression e, int depth = 0, int parentNodeId = 0)
        {
            if (e == null) return;

            Console.WriteLine((new string('\t', depth)) + "-------------");
            Console.WriteLine((new string('\t', depth)) + e.NodeType);
            Console.WriteLine((new string('\t', depth)) + e.Type);
            Console.WriteLine((new string('\t', depth)) + "-------------");

            foreach (var child in GetChildExpressions(e))
            {
                PrintPreOrder(child, depth + 1, parentNodeId + 1);
            }
        }

        public static List<Expression> GetChildExpressions(Expression expression)
        {
            // Ensure the given argument is not null
            if (expression == null) return new List<Expression>();

            // Retrieve properties of the expression type
            var properties = expression.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Inspect each property and filter those which return an Expression or a collection of Expressions
            var childExpressions = new List<Expression>();
            foreach (var property in properties)
            {
                if (typeof(Expression).IsAssignableFrom(property.PropertyType))
                {
                    var child = property.GetValue(expression) as Expression;
                    if (child != null)
                        childExpressions.Add(child);
                }
                else if (typeof(IEnumerable<Expression>).IsAssignableFrom(property.PropertyType))
                {
                    var children = property.GetValue(expression) as IEnumerable<Expression>;
                    if (children != null)
                        childExpressions.AddRange(children);
                }
            }

            return childExpressions;
        }

        public static void PostExpressionToDatabase(Expression parseTree)
        {
            string connectionString = "Server=dev1.layeronesoftware.com;Database=FTBTrade;Integrated Security=SSPI;";
            DatabaseInteraction dbInteraction = new DatabaseInteraction(connectionString);
            dbInteraction.OpenConnection();
            dbInteraction.PostExpressionToDatabase(parseTree);
            dbInteraction.CloseConnection();
        }
    }
}

