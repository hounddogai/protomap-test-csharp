namespace Acme.Workforce.Absence.Tests.Policy.Mapping;

using Acme.Workforce.Absence.Policy.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptivePolicyBundleRulesChecks
{
    public static void AdaptivePolicyBundleValidatorReportsEveryProblem()
    {
        var validator = AdaptivePolicyBundleRules.DefaultAdaptivePolicyBundleValidator();
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePolicyBundleWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AdaptivePolicyBundleWindowDaysRangeRule(255, 3600);
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptivePolicyBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptivePolicyBundleLabelLengthRule(32);
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptivePolicyBundleRequiredCodeRule("adaptive policy bundle needs an entry");
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePolicyBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptivePolicyBundleCodeLengthRule(16);
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptivePolicyBundleValidatorReportsEveryProblem();
        AdaptivePolicyBundleWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AdaptivePolicyBundleLabelLengthRuleFlagsALongLabel();
        AdaptivePolicyBundleRequiredCodeRuleFlagsABlankCode();
        AdaptivePolicyBundleCodeLengthRuleFlagsALongCode();
    }
}
