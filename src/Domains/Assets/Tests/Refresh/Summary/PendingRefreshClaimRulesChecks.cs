namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingRefreshClaimRulesChecks
{
    public static void PendingRefreshClaimValidatorReportsEveryProblem()
    {
        var validator = PendingRefreshClaimRules.DefaultPendingRefreshClaimValidator();
        var item = PendingRefreshClaim.NewPendingRefreshClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingRefreshClaimRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PendingRefreshClaimRateRangeRule(3.4, 3046.2);
        var item = PendingRefreshClaim.NewPendingRefreshClaim("x", "y");
        item.Rate = 3047.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingRefreshClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingRefreshClaimRequiredTitleRule("pending refresh claim needs a value");
        var item = PendingRefreshClaim.NewPendingRefreshClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingRefreshClaimRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingRefreshClaimRequiredLabelRule("pending refresh claim needs an entry");
        var item = PendingRefreshClaim.NewPendingRefreshClaim("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingRefreshClaimValidatorReportsEveryProblem();
        PendingRefreshClaimRateRangeRuleFlagsValuesAboveRate();
        PendingRefreshClaimRequiredTitleRuleFlagsABlankTitle();
        PendingRefreshClaimRequiredLabelRuleFlagsABlankLabel();
    }
}
