namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedBonusPoolThresholdRulesChecks
{
    public static void BatchedBonusPoolThresholdValidatorReportsEveryProblem()
    {
        var validator = BatchedBonusPoolThresholdRules.DefaultBatchedBonusPoolThresholdValidator();
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedBonusPoolThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedBonusPoolThresholdRequiredCodeRule("batched bonus pool threshold needs this field");
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedBonusPoolThresholdRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new BatchedBonusPoolThresholdRequiredExternalRefRule("batched bonus pool threshold needs a value");
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedBonusPoolThresholdCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new BatchedBonusPoolThresholdCeilingRangeRule(7.5, 127.9);
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("x", "y");
        item.Ceiling = 129.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedBonusPoolThresholdRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new BatchedBonusPoolThresholdRequiredUnitRule("batched bonus pool threshold needs an entry");
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedBonusPoolThresholdExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new BatchedBonusPoolThresholdExternalRefLengthRule(16);
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedBonusPoolThresholdValidatorReportsEveryProblem();
        BatchedBonusPoolThresholdRequiredCodeRuleFlagsABlankCode();
        BatchedBonusPoolThresholdRequiredExternalRefRuleFlagsABlankExternalRef();
        BatchedBonusPoolThresholdCeilingRangeRuleFlagsValuesAboveCeiling();
        BatchedBonusPoolThresholdRequiredUnitRuleFlagsABlankUnit();
        BatchedBonusPoolThresholdExternalRefLengthRuleFlagsALongExternalRef();
    }
}
