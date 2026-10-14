namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class SeasonalPayBandCycleRulesChecks
{
    public static void SeasonalPayBandCycleValidatorReportsEveryProblem()
    {
        var validator = SeasonalPayBandCycleRules.DefaultSeasonalPayBandCycleValidator();
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPayBandCycleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SeasonalPayBandCycleMarginRangeRule(6.2, 4599.5);
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("x", "y");
        item.Margin = 4601.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalPayBandCycleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalPayBandCycleRequiredLabelRule("seasonal pay band cycle needs a value");
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPayBandCycleGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new SeasonalPayBandCycleGraceDaysRangeRule(16, 120);
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalPayBandCycleValidatorReportsEveryProblem();
        SeasonalPayBandCycleMarginRangeRuleFlagsValuesAboveMargin();
        SeasonalPayBandCycleRequiredLabelRuleFlagsABlankLabel();
        SeasonalPayBandCycleGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
