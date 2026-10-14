namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingMeritCycleSegmentRulesChecks
{
    public static void RollingMeritCycleSegmentValidatorReportsEveryProblem()
    {
        var validator = RollingMeritCycleSegmentRules.DefaultRollingMeritCycleSegmentValidator();
        var item = RollingMeritCycleSegment.NewRollingMeritCycleSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingMeritCycleSegmentMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new RollingMeritCycleSegmentMaximumRangeRule(0.6, 2539.0);
        var item = RollingMeritCycleSegment.NewRollingMeritCycleSegment("x", "y");
        item.Maximum = 2540.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingMeritCycleSegmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingMeritCycleSegmentLabelLengthRule(120);
        var item = RollingMeritCycleSegment.NewRollingMeritCycleSegment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingMeritCycleSegmentValidatorReportsEveryProblem();
        RollingMeritCycleSegmentMaximumRangeRuleFlagsValuesAboveMaximum();
        RollingMeritCycleSegmentLabelLengthRuleFlagsALongLabel();
    }
}
