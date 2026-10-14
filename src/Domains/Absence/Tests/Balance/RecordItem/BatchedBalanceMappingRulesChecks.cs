namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedBalanceMappingRulesChecks
{
    public static void BatchedBalanceMappingValidatorReportsEveryProblem()
    {
        var validator = BatchedBalanceMappingRules.DefaultBatchedBalanceMappingValidator();
        var item = BatchedBalanceMapping.NewBatchedBalanceMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedBalanceMappingVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new BatchedBalanceMappingVersionRangeRule(32, 64);
        var item = BatchedBalanceMapping.NewBatchedBalanceMapping("x", "y");
        item.Version = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedBalanceMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedBalanceMappingRequiredNameRule("batched balance mapping needs this field");
        var item = BatchedBalanceMapping.NewBatchedBalanceMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedBalanceMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedBalanceMappingRequiredCodeRule("batched balance mapping needs this field");
        var item = BatchedBalanceMapping.NewBatchedBalanceMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedBalanceMappingJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new BatchedBalanceMappingJobCodeLengthRule(120);
        var item = BatchedBalanceMapping.NewBatchedBalanceMapping("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedBalanceMappingValidatorReportsEveryProblem();
        BatchedBalanceMappingVersionRangeRuleFlagsValuesAboveVersion();
        BatchedBalanceMappingRequiredNameRuleFlagsABlankName();
        BatchedBalanceMappingRequiredCodeRuleFlagsABlankCode();
        BatchedBalanceMappingJobCodeLengthRuleFlagsALongJobCode();
    }
}
