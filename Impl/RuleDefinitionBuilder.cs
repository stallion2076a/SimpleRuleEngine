using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace RuleEngineLib
{
    public static class RuleDefinitionBuilder
    {
        public static RuleDefinition ParseRule(String LHS, String Operator, String RHS)
        {
            string[] RHSArr;
            RuleDefinition Rd = null;
            switch (Operator)
            {
                case "CONTAINSANY":
                    RHSArr = Regex.Split(RHS, ",");
                    Rd = new RuleDefinition(LHS, ContainsType.ContainsAny, RHSArr);
                    break;
                case "CONTAINSALL":
                    RHSArr = Regex.Split(RHS, ",");
                    Rd = new RuleDefinition(LHS, ContainsType.ContainsAll, RHSArr);
                    break;
                case "CONTAINSNONE":
                    RHSArr = Regex.Split(RHS, ",");
                    Rd = new RuleDefinition(LHS, ContainsType.DoesNotContainAny, RHSArr);
                    break;
                case "IN":
                    RHSArr = Regex.Split(RHS, ",");
                    Rd = new RuleDefinition(LHS, ExpressionType.Equal, RHSArr);
                    break;
                case "NOTIN":
                    RHSArr = Regex.Split(RHS, ",");
                    Rd = new RuleDefinition(LHS, ExpressionType.NotEqual, RHSArr);
                    break;
                case "EQ":
                    Rd = new RuleDefinition(LHS, ExpressionType.Equal, RHS);
                    break;
                case "NOTEQ":
                    Rd = new RuleDefinition(LHS, ExpressionType.NotEqual, RHS);
                    break;
                case "GT":
                    Rd = new RuleDefinition(LHS, ExpressionType.GreaterThan, RHS);
                    break;
                case "LT":
                    Rd = new RuleDefinition(LHS, ExpressionType.LessThan, RHS);
                    break;
                case "GTE":
                    Rd = new RuleDefinition(LHS, ExpressionType.GreaterThanOrEqual, RHS);
                    break;
                case "LTE":
                    Rd = new RuleDefinition(LHS, ExpressionType.LessThanOrEqual, RHS);
                    break;
            }
            return Rd;
        }
    }
}
