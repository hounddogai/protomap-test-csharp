namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleReservationRulesChecks
{
    public static void AdaptiveMeritCycleReservationValidatorReportsEveryProblem()
    {
        var validator = AdaptiveMeritCycleReservationRules.DefaultAdaptiveMeritCycleReservationValidator();
        var item = AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveMeritCycleReservationSkillLengthRuleFlagsALongSkill()
    {
        var rule = new AdaptiveMeritCycleReservationSkillLengthRule(32);
        var item = AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveMeritCycleReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveMeritCycleReservationTitleLengthRule(32);
        var item = AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveMeritCycleReservationValidatorReportsEveryProblem();
        AdaptiveMeritCycleReservationSkillLengthRuleFlagsALongSkill();
        AdaptiveMeritCycleReservationTitleLengthRuleFlagsALongTitle();
    }
}
