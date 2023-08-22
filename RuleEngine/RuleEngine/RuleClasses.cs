using System;

namespace RuleDatabaseObjects
{
    internal class RuleType
    {
        public int RuleTypeId { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public int? Version { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string ApplicationSource { get; set; }
        public string SystemSource { get; set; }
        public int? GlobalVersion { get; set; }

        // Other database operations can be added similarly...
    }

    internal class Rule
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; }
        public int RuleTypeId { get; set; }
        public string RuleCode { get; set; }
        public DateTime? Created { get; set; }
        public int? Version { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string ApplicationSource { get; set; }
        public string SystemSource { get; set; }
        public int? GlobalVersion { get; set; }

        // Other database operations can be added similarly...
    }

    internal class ExpressionType
    {
        public int ExpressionTypeId { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public int? Version { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string ApplicationSource { get; set; }
        public string SystemSource { get; set; }
        public int? GlobalVersion { get; set; }

        // Other database operations can be added similarly...
    }

    internal class ExpressionValueType
    {
        public int ExpressionValueTypeId { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public int? Version { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string ApplicationSource { get; set; }
        public string SystemSource { get; set; }
        public int? GlobalVersion { get; set; }

        // Other database operations can be added similarly...
    }

    

    internal class Expression
    {
        public int ExpressionId { get; set; }
        public int RuleId { get; set; }
        public int? ParentExpressionId { get; set; }
        public int? ExpressionSequence { get; set; }
        public int ExpressionTypeId { get; set; }
        public int? ExpressionCallRuleId { get; set; }
        public string ExpressionValue { get; set; }
        public int? ExpressionValueType { get; set; }
        public DateTime? Created { get; set; }
        public int? Version { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string ApplicationSource { get; set; }
        public string SystemSource { get; set; }
        public int? GlobalVersion { get; set; }

        // Other database operations can be added similarly...
    }
}
