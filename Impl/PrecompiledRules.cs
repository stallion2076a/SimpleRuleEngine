using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RuleEngineLib
{
    public static class PrecompiledRules
    {
        ///
        /// A method used to precompile rules for a provided type
        /// 
        public static List<Func<T, bool>> CompileRule<T>(T targetEntity, List<RuleDefinition> rules)
        {
            var compiledRules = new List<Func<T, bool>>();

            // Compile Rules
            rules.ForEach(rule =>
            {
                var genericType = Expression.Parameter(typeof(T));
                var key = MemberExpression.Property(genericType, rule.ComparisonPredicate);
                var propertyType = typeof(T).GetProperty(rule.ComparisonPredicate).PropertyType;
                var value = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, propertyType));
                var binaryExpression = Expression.MakeBinary(rule.ComparisonOperator, key, value);

                compiledRules.Add(Expression.Lambda<Func<T, bool>>(binaryExpression, genericType).Compile());
            });


            return compiledRules;
        }

        public static Expression<Func<T, bool>> CompileRule<T>(T targetEntity, RuleDefinition rule)
        {
            // Compile Rule
            var genericType = Expression.Parameter(typeof(T));
            var key = MemberExpression.Property(genericType, rule.ComparisonPredicate);
            var propertyType = typeof(T).GetProperty(rule.ComparisonPredicate).PropertyType;
            Type underlyingType = null;
            if (Nullable.GetUnderlyingType(propertyType) != null)
                underlyingType = Nullable.GetUnderlyingType(propertyType);

            //Contains Expression
            if (rule.ContainsTypeOperator != null)
            {
                //MethodInfo
                var mi = typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 1);
                if (rule.keywords != null && rule.keywords.Count() > 0)
                {
                    var value = Expression.Constant(Convert.ChangeType(rule.keywords.First().Trim(), typeof(string)));
                    //Method Call Expression
                    var Mcexp = Expression.Call(key, mi, value);
                    var exp = Expression.Lambda<Func<T, bool>>(Mcexp, genericType);
                    if (rule.ContainsTypeOperator == ContainsType.DoesNotContainAny)
                        exp = exp.Not();
                    foreach (string keyword in rule.keywords.Skip(1))
                    {
                        string temp = keyword.Trim();
                        value = Expression.Constant(Convert.ChangeType(temp, typeof(string)));
                        Mcexp = Expression.Call(key, mi, value);
                        var subexp = Expression.Lambda<Func<T, bool>>(Mcexp, genericType);

                        if (rule.ContainsTypeOperator == ContainsType.DoesNotContainAny)
                            subexp = subexp.Not();
                        if (rule.ContainsTypeOperator == ContainsType.ContainsAll || rule.ContainsTypeOperator == ContainsType.DoesNotContainAny)
                            exp = exp.And(subexp);
                        else if (rule.ContainsTypeOperator == ContainsType.ContainsAny)
                            exp = exp.Or(subexp);
                    }
                    return exp;
                }
                else
                {
                    var value = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, typeof(string)));
                    var Mcexp = Expression.Call(key, mi, value);
                    var exp = Expression.Lambda<Func<T, bool>>(Mcexp, genericType);
                    if (rule.ContainsTypeOperator == ContainsType.DoesNotContainAny)
                        exp = exp.Not();
                    return exp;
                }
            }

            //Binary Expressions
            Expression binaryExpression = null;
            if (rule.keywords != null && rule.keywords.Count() > 0)
            {
                Expression value = Expression.Constant(Convert.ChangeType(rule.keywords.First().Trim(), propertyType));
                binaryExpression = Expression.MakeBinary(rule.ComparisonOperator, key, value);
                foreach (string keyword in rule.keywords.Skip(1))
                {
                    string temp = keyword.Trim();
                    value = Expression.Constant(Convert.ChangeType(temp, propertyType));
                    if (rule.ComparisonOperator == ExpressionType.Equal)
                        binaryExpression = Expression.OrElse(binaryExpression, Expression.MakeBinary(rule.ComparisonOperator, key, value));
                    else
                        binaryExpression = Expression.AndAlso(binaryExpression, Expression.MakeBinary(rule.ComparisonOperator, key, value));

                }
            }
            else
            {
                Expression value = null;
                if (underlyingType != null)
                {
                    var UnderlyingValue = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, underlyingType));
                    value = Expression.Convert(UnderlyingValue, propertyType);
                }
                else
                    value = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, propertyType));

                binaryExpression = Expression.MakeBinary(rule.ComparisonOperator, key, value);
            }
            return Expression.Lambda<Func<T, bool>>(binaryExpression, genericType);
        }



        public static Expression<Func<T, bool>> AddContains<T>(T targetEntity, RuleDefinition rule)
        {
            var mi = typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 1);
            var genericType = Expression.Parameter(typeof(T));
            var key = MemberExpression.Property(genericType, rule.ComparisonPredicate);
            var propertyType = typeof(T).GetProperty(rule.ComparisonPredicate).PropertyType;
            var value = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, propertyType));
            var exp = Expression.Call(key, mi, value);
            return Expression.Lambda<Func<T, bool>>(exp, genericType);
        }

        public static Func<T, T2, bool> CompileRule<T, T2>(T sourceEntityLHS, T2 sourceEntityRHS, RuleDefinition rule)
        {
            // Compile Rule
            var genericTypeLHS = Expression.Parameter(typeof(T));
            var genericTypeRHS = Expression.Parameter(typeof(T2));
            var LHS = MemberExpression.Property(genericTypeLHS, rule.ComparisonPredicate);
            var propertyTypeLHS = typeof(T).GetProperty(rule.ComparisonPredicate).PropertyType;
            var RHS = MemberExpression.Property(genericTypeRHS, rule.ComparisonValue);
            var propertyTypeRHS = typeof(T2).GetProperty(rule.ComparisonValue).PropertyType;
            var binaryExpression = Expression.MakeBinary(rule.ComparisonOperator, LHS, RHS);

            return Expression.Lambda<Func<T, T2, bool>>(binaryExpression, genericTypeLHS, genericTypeRHS).Compile();
        }

        public static Func<TEntity, bool> IsCurrent<TEntity>(TEntity sourceEntity)
        {
            var genericType = Expression.Parameter(typeof(TEntity));
            var ValidFrom = MemberExpression.Property(genericType, "EffectiveStartDate");
            var ValidTo = MemberExpression.Property(genericType, "EffectiveEndDate");
            var value = Expression.Constant(Convert.ChangeType(DateTime.Now, TypeCode.DateTime));
            var binaryExpression = Expression.MakeBinary(ExpressionType.LessThanOrEqual, ValidFrom, value);
            binaryExpression = Expression.AndAlso(binaryExpression, Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, ValidTo, value));

            return Expression.Lambda<Func<TEntity, bool>>(binaryExpression, genericType).Compile();
        }



        public static Expression<Func<T, bool>> Begin<T>(bool value = false)
        {
            if (value)
                return parameter => true; //value cannot be used in place of true/false

            return parameter => false;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            return CombineLambdas(left, right, ExpressionType.AndAlso);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            return CombineLambdas(left, right, ExpressionType.OrElse);
        }


        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expr)
        {
            return Expression.Lambda<Func<T, bool>>(
                Expression.Not(expr.Body), expr.Parameters[0]);
        }

        #region private

        private static Expression<Func<T, bool>> CombineLambdas<T>(this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right, ExpressionType expressionType)
        {
            //Remove expressions created with Begin<T>()
            if (IsExpressionBodyConstant(left))
                return (right);

            ParameterExpression p = left.Parameters[0];

            SubstituteParameterVisitor visitor = new SubstituteParameterVisitor();
            visitor.Sub[right.Parameters[0]] = p;

            Expression body = Expression.MakeBinary(expressionType, left.Body, visitor.Visit(right.Body));
            return Expression.Lambda<Func<T, bool>>(body, p);
        }



        private static bool IsExpressionBodyConstant<T>(Expression<Func<T, bool>> left)
        {
            return left.Body.NodeType == ExpressionType.Constant;
        }

        internal class SubstituteParameterVisitor : ExpressionVisitor
        {
            public Dictionary<Expression, Expression> Sub = new Dictionary<Expression, Expression>();

            protected override Expression VisitParameter(ParameterExpression node)
            {
                Expression newValue;
                if (Sub.TryGetValue(node, out newValue))
                {
                    return newValue;
                }
                return node;
            }
        }

        #endregion
    }

}
