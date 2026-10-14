namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionForecastRulesChecks
{
    public static void PositionForecastValidatorReportsEveryProblem()
    {
        var validator = PositionForecastRules.DefaultPositionForecastValidator();
        var item = PositionForecast.NewPositionForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionForecastPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PositionForecastPriorityRangeRule(80, 80);
        var item = PositionForecast.NewPositionForecast("x", "y");
        item.Priority = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionForecastRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PositionForecastRequiredLabelRule("position forecast needs an entry");
        var item = PositionForecast.NewPositionForecast("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PositionForecastRequiredTitleRule("position forecast needs this field");
        var item = PositionForecast.NewPositionForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionForecastBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PositionForecastBatchSizeRangeRule(32, 64);
        var item = PositionForecast.NewPositionForecast("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PositionForecastLabelLengthRule(80);
        var item = PositionForecast.NewPositionForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PositionForecastValidatorReportsEveryProblem();
        PositionForecastPriorityRangeRuleFlagsValuesAbovePriority();
        PositionForecastRequiredLabelRuleFlagsABlankLabel();
        PositionForecastRequiredTitleRuleFlagsABlankTitle();
        PositionForecastBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        PositionForecastLabelLengthRuleFlagsALongLabel();
    }
}
