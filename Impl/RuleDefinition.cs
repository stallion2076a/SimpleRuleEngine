using System.Linq.Expressions;

namespace RuleEngineLib
{
    public class RuleDefinition
    {
        public string ComparisonPredicate { get; set; }
        public ExpressionType ComparisonOperator { get; set; }
        public ContainsType? ContainsTypeOperator { get; set; }
        public string ComparisonValue { get; set; }
        public string[] keywords { get; set; }
        /// 
        /// The rule method that 
        /// 
        public RuleDefinition(string comparisonPredicate, ExpressionType comparisonOperator, string comparisonValue)
        {
            ComparisonPredicate = comparisonPredicate;
            ComparisonOperator = comparisonOperator;
            ComparisonValue = comparisonValue;
        }

        public RuleDefinition(string comparisonPredicate, ExpressionType comparisonOperator, params string[] keywords)
        {
            ComparisonPredicate = comparisonPredicate;
            ComparisonOperator = comparisonOperator;
            this.keywords = keywords;
        }

        public RuleDefinition(string comparisonPredicate, ContainsType comparisonOperator, string comparisonValue)
        {
            ComparisonPredicate = comparisonPredicate;
            ContainsTypeOperator = comparisonOperator;
            ComparisonValue = comparisonValue;
        }

        public RuleDefinition(string comparisonPredicate, ContainsType comparisonOperator, params string[] keywords)
        {
            ComparisonPredicate = comparisonPredicate;
            ContainsTypeOperator = comparisonOperator;
            this.keywords = keywords;
        }
    }
}
