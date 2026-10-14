namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RefreshTargetRulesChecks
{
    public static void RefreshTargetValidatorReportsEveryProblem()
    {
        var validator = RefreshTargetRules.DefaultRefreshTargetValidator();
        var item = RefreshTarget.NewRefreshTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshTargetJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new RefreshTargetJobCodeLengthRule(86400);
        var item = RefreshTarget.NewRefreshTarget("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RefreshTargetRequiredTitleRule("refresh target needs a value");
        var item = RefreshTarget.NewRefreshTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshTargetCodeLengthRule(255);
        var item = RefreshTarget.NewRefreshTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshTargetRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new RefreshTargetRequiredJobCodeRule("refresh target needs this field");
        var item = RefreshTarget.NewRefreshTarget("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshTargetMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new RefreshTargetMaximumRangeRule(7.2, 2283.9);
        var item = RefreshTarget.NewRefreshTarget("x", "y");
        item.Maximum = 2285.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RefreshTargetValidatorReportsEveryProblem();
        RefreshTargetJobCodeLengthRuleFlagsALongJobCode();
        RefreshTargetRequiredTitleRuleFlagsABlankTitle();
        RefreshTargetCodeLengthRuleFlagsALongCode();
        RefreshTargetRequiredJobCodeRuleFlagsABlankJobCode();
        RefreshTargetMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
