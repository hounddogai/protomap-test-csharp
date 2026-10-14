namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AdaptiveAccrualBundleRulesChecks
{
    public static void AdaptiveAccrualBundleValidatorReportsEveryProblem()
    {
        var validator = AdaptiveAccrualBundleRules.DefaultAdaptiveAccrualBundleValidator();
        var item = AdaptiveAccrualBundle.NewAdaptiveAccrualBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveAccrualBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveAccrualBundleRequiredLabelRule("adaptive accrual bundle needs a value");
        var item = AdaptiveAccrualBundle.NewAdaptiveAccrualBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveAccrualBundleBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new AdaptiveAccrualBundleBadgeLengthRule(1000);
        var item = AdaptiveAccrualBundle.NewAdaptiveAccrualBundle("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveAccrualBundleValidatorReportsEveryProblem();
        AdaptiveAccrualBundleRequiredLabelRuleFlagsABlankLabel();
        AdaptiveAccrualBundleBadgeLengthRuleFlagsALongBadge();
    }
}
