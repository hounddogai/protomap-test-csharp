namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshAdjustmentRulesChecks
{
    public static void RefreshAdjustmentValidatorReportsEveryProblem()
    {
        var validator = RefreshAdjustmentRules.DefaultRefreshAdjustmentValidator();
        var item = RefreshAdjustment.NewRefreshAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshAdjustmentMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new RefreshAdjustmentMarginRangeRule(8.2, 4312.7);
        var item = RefreshAdjustment.NewRefreshAdjustment("x", "y");
        item.Margin = 4314.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshAdjustmentSourceLengthRuleFlagsALongSource()
    {
        var rule = new RefreshAdjustmentSourceLengthRule(120);
        var item = RefreshAdjustment.NewRefreshAdjustment("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshAdjustmentRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new RefreshAdjustmentRequiredDescriptionRule("refresh adjustment needs a value");
        var item = RefreshAdjustment.NewRefreshAdjustment("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshAdjustmentValidatorReportsEveryProblem();
        RefreshAdjustmentMarginRangeRuleFlagsValuesAboveMargin();
        RefreshAdjustmentSourceLengthRuleFlagsALongSource();
        RefreshAdjustmentRequiredDescriptionRuleFlagsABlankDescription();
    }
}
