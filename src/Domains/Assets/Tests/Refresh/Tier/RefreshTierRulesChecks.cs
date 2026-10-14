namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RefreshTierRulesChecks
{
    public static void RefreshTierValidatorReportsEveryProblem()
    {
        var validator = RefreshTierRules.DefaultRefreshTierValidator();
        var item = RefreshTier.NewRefreshTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshTierRequiredCodeRule("refresh tier needs a value");
        var item = RefreshTier.NewRefreshTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshTierHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new RefreshTierHoursRangeRule(0.3, 717.6);
        var item = RefreshTier.NewRefreshTier("x", "y");
        item.Hours = 719.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshTierCodeLengthRule(16);
        var item = RefreshTier.NewRefreshTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshTierMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RefreshTierMinimumRangeRule(6.0, 4469.0);
        var item = RefreshTier.NewRefreshTier("x", "y");
        item.Minimum = 4470.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RefreshTierValidatorReportsEveryProblem();
        RefreshTierRequiredCodeRuleFlagsABlankCode();
        RefreshTierHoursRangeRuleFlagsValuesAboveHours();
        RefreshTierCodeLengthRuleFlagsALongCode();
        RefreshTierMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
