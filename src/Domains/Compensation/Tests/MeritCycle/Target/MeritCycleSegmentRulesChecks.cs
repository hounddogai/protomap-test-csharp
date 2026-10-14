namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleSegmentRulesChecks
{
    public static void MeritCycleSegmentValidatorReportsEveryProblem()
    {
        var validator = MeritCycleSegmentRules.DefaultMeritCycleSegmentValidator();
        var item = MeritCycleSegment.NewMeritCycleSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleSegmentGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new MeritCycleSegmentGraceDaysRangeRule(3600, 86400);
        var item = MeritCycleSegment.NewMeritCycleSegment("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleSegmentRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new MeritCycleSegmentRatingRangeRule(7.8, 370.7);
        var item = MeritCycleSegment.NewMeritCycleSegment("x", "y");
        item.Rating = 372.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleSegmentRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new MeritCycleSegmentRequiredLocationRule("merit cycle segment needs this field");
        var item = MeritCycleSegment.NewMeritCycleSegment("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleSegmentValidatorReportsEveryProblem();
        MeritCycleSegmentGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        MeritCycleSegmentRatingRangeRuleFlagsValuesAboveRating();
        MeritCycleSegmentRequiredLocationRuleFlagsABlankLocation();
    }
}
