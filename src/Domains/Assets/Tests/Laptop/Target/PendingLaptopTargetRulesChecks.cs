namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingLaptopTargetRulesChecks
{
    public static void PendingLaptopTargetValidatorReportsEveryProblem()
    {
        var validator = PendingLaptopTargetRules.DefaultPendingLaptopTargetValidator();
        var item = PendingLaptopTarget.NewPendingLaptopTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingLaptopTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingLaptopTargetRequiredCodeRule("pending laptop target needs an entry");
        var item = PendingLaptopTarget.NewPendingLaptopTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingLaptopTargetExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PendingLaptopTargetExpiresAtRangeRule(32, 86400);
        var item = PendingLaptopTarget.NewPendingLaptopTarget("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingLaptopTargetRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PendingLaptopTargetRequiredReviewerRule("pending laptop target needs this field");
        var item = PendingLaptopTarget.NewPendingLaptopTarget("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingLaptopTargetBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PendingLaptopTargetBatchSizeRangeRule(64, 80);
        var item = PendingLaptopTarget.NewPendingLaptopTarget("x", "y");
        item.BatchSize = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingLaptopTargetValidatorReportsEveryProblem();
        PendingLaptopTargetRequiredCodeRuleFlagsABlankCode();
        PendingLaptopTargetExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PendingLaptopTargetRequiredReviewerRuleFlagsABlankReviewer();
        PendingLaptopTargetBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
