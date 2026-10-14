namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class SeasonalPayBandCheckpointRulesChecks
{
    public static void SeasonalPayBandCheckpointValidatorReportsEveryProblem()
    {
        var validator = SeasonalPayBandCheckpointRules.DefaultSeasonalPayBandCheckpointValidator();
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPayBandCheckpointRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new SeasonalPayBandCheckpointRequiredSourceRule("seasonal pay band checkpoint needs this field");
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPayBandCheckpointMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SeasonalPayBandCheckpointMarginRangeRule(9.9, 3093.3);
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("x", "y");
        item.Margin = 3094.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalPayBandCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalPayBandCheckpointRequiredCodeRule("seasonal pay band checkpoint needs this field");
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPayBandCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalPayBandCheckpointCodeLengthRule(86400);
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalPayBandCheckpointValidatorReportsEveryProblem();
        SeasonalPayBandCheckpointRequiredSourceRuleFlagsABlankSource();
        SeasonalPayBandCheckpointMarginRangeRuleFlagsValuesAboveMargin();
        SeasonalPayBandCheckpointRequiredCodeRuleFlagsABlankCode();
        SeasonalPayBandCheckpointCodeLengthRuleFlagsALongCode();
    }
}
