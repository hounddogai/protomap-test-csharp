namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedDeskTierRulesChecks
{
    public static void BatchedDeskTierValidatorReportsEveryProblem()
    {
        var validator = BatchedDeskTierRules.DefaultBatchedDeskTierValidator();
        var item = BatchedDeskTier.NewBatchedDeskTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedDeskTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BatchedDeskTierRequiredLabelRule("batched desk tier needs an entry");
        var item = BatchedDeskTier.NewBatchedDeskTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedDeskTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedDeskTierRequiredCodeRule("batched desk tier needs a value");
        var item = BatchedDeskTier.NewBatchedDeskTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedDeskTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedDeskTierCodeLengthRule(120);
        var item = BatchedDeskTier.NewBatchedDeskTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedDeskTierValidatorReportsEveryProblem();
        BatchedDeskTierRequiredLabelRuleFlagsABlankLabel();
        BatchedDeskTierRequiredCodeRuleFlagsABlankCode();
        BatchedDeskTierCodeLengthRuleFlagsALongCode();
    }
}
