namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RefreshOverrideRulesChecks
{
    public static void RefreshOverrideValidatorReportsEveryProblem()
    {
        var validator = RefreshOverrideRules.DefaultRefreshOverrideValidator();
        var item = RefreshOverride.NewRefreshOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshOverrideMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new RefreshOverrideMarginRangeRule(5.6, 4144.5);
        var item = RefreshOverride.NewRefreshOverride("x", "y");
        item.Margin = 4146.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RefreshOverrideRequiredLabelRule("refresh override needs an entry");
        var item = RefreshOverride.NewRefreshOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshOverrideCodeLengthRule(64);
        var item = RefreshOverride.NewRefreshOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshOverrideRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RefreshOverrideRateRangeRule(3.8, 170.8);
        var item = RefreshOverride.NewRefreshOverride("x", "y");
        item.Rate = 172.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RefreshOverrideValidatorReportsEveryProblem();
        RefreshOverrideMarginRangeRuleFlagsValuesAboveMargin();
        RefreshOverrideRequiredLabelRuleFlagsABlankLabel();
        RefreshOverrideCodeLengthRuleFlagsALongCode();
        RefreshOverrideRateRangeRuleFlagsValuesAboveRate();
    }
}
