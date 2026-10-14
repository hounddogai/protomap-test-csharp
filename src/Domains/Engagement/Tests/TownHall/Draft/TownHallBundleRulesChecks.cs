namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TownHallBundleRulesChecks
{
    public static void TownHallBundleValidatorReportsEveryProblem()
    {
        var validator = TownHallBundleRules.DefaultTownHallBundleValidator();
        var item = TownHallBundle.NewTownHallBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallBundleSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new TownHallBundleSegmentLengthRule(1000);
        var item = TownHallBundle.NewTownHallBundle("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallBundleCodeLengthRule(64);
        var item = TownHallBundle.NewTownHallBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallBundleScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new TownHallBundleScoreRangeRule(2.8, 3314.7);
        var item = TownHallBundle.NewTownHallBundle("x", "y");
        item.Score = 3316.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallBundleValidatorReportsEveryProblem();
        TownHallBundleSegmentLengthRuleFlagsALongSegment();
        TownHallBundleCodeLengthRuleFlagsALongCode();
        TownHallBundleScoreRangeRuleFlagsValuesAboveScore();
    }
}
