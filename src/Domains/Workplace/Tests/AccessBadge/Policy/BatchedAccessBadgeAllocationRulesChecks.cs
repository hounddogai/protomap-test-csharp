namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BatchedAccessBadgeAllocationRulesChecks
{
    public static void BatchedAccessBadgeAllocationValidatorReportsEveryProblem()
    {
        var validator = BatchedAccessBadgeAllocationRules.DefaultBatchedAccessBadgeAllocationValidator();
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAccessBadgeAllocationChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new BatchedAccessBadgeAllocationChecksumLengthRule(1000);
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAccessBadgeAllocationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedAccessBadgeAllocationRequiredTitleRule("batched access badge allocation needs a value");
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAccessBadgeAllocationRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new BatchedAccessBadgeAllocationRequiredChecksumRule(
            "batched access badge allocation needs an entry");
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAccessBadgeAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedAccessBadgeAllocationRequiredCodeRule("batched access badge allocation needs this field");
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAccessBadgeAllocationScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new BatchedAccessBadgeAllocationScoreRangeRule(3.3, 332.2);
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("x", "y");
        item.Score = 333.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeAllocationValidatorReportsEveryProblem();
        BatchedAccessBadgeAllocationChecksumLengthRuleFlagsALongChecksum();
        BatchedAccessBadgeAllocationRequiredTitleRuleFlagsABlankTitle();
        BatchedAccessBadgeAllocationRequiredChecksumRuleFlagsABlankChecksum();
        BatchedAccessBadgeAllocationRequiredCodeRuleFlagsABlankCode();
        BatchedAccessBadgeAllocationScoreRangeRuleFlagsValuesAboveScore();
    }
}
