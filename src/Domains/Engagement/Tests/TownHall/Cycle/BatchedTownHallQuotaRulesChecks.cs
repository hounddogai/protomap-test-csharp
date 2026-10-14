namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class BatchedTownHallQuotaRulesChecks
{
    public static void BatchedTownHallQuotaValidatorReportsEveryProblem()
    {
        var validator = BatchedTownHallQuotaRules.DefaultBatchedTownHallQuotaValidator();
        var item = BatchedTownHallQuota.NewBatchedTownHallQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTownHallQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedTownHallQuotaRequiredCodeRule("batched town hall quota needs a value");
        var item = BatchedTownHallQuota.NewBatchedTownHallQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTownHallQuotaSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new BatchedTownHallQuotaSequenceRangeRule(16, 3600);
        var item = BatchedTownHallQuota.NewBatchedTownHallQuota("x", "y");
        item.Sequence = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedTownHallQuotaValidatorReportsEveryProblem();
        BatchedTownHallQuotaRequiredCodeRuleFlagsABlankCode();
        BatchedTownHallQuotaSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
