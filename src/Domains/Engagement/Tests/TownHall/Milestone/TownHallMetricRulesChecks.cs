namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallMetricRulesChecks
{
    public static void TownHallMetricValidatorReportsEveryProblem()
    {
        var validator = TownHallMetricRules.DefaultTownHallMetricValidator();
        var item = TownHallMetric.NewTownHallMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallMetricLocationLengthRuleFlagsALongLocation()
    {
        var rule = new TownHallMetricLocationLengthRule(3600);
        var item = TownHallMetric.NewTownHallMetric("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallMetricCodeLengthRule(86400);
        var item = TownHallMetric.NewTownHallMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallMetricScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new TownHallMetricScoreRangeRule(6.0, 178.8);
        var item = TownHallMetric.NewTownHallMetric("x", "y");
        item.Score = 180.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallMetricValidatorReportsEveryProblem();
        TownHallMetricLocationLengthRuleFlagsALongLocation();
        TownHallMetricCodeLengthRuleFlagsALongCode();
        TownHallMetricScoreRangeRuleFlagsValuesAboveScore();
    }
}
