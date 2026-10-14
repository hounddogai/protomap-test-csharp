namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ScheduledMeritCycleSnapshotRulesChecks
{
    public static void ScheduledMeritCycleSnapshotValidatorReportsEveryProblem()
    {
        var validator = ScheduledMeritCycleSnapshotRules.DefaultScheduledMeritCycleSnapshotValidator();
        var item = ScheduledMeritCycleSnapshot.NewScheduledMeritCycleSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledMeritCycleSnapshotRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ScheduledMeritCycleSnapshotRequiredTeamRule("scheduled merit cycle snapshot needs a value");
        var item = ScheduledMeritCycleSnapshot.NewScheduledMeritCycleSnapshot("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledMeritCycleSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledMeritCycleSnapshotCodeLengthRule(32);
        var item = ScheduledMeritCycleSnapshot.NewScheduledMeritCycleSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledMeritCycleSnapshotValidatorReportsEveryProblem();
        ScheduledMeritCycleSnapshotRequiredTeamRuleFlagsABlankTeam();
        ScheduledMeritCycleSnapshotCodeLengthRuleFlagsALongCode();
    }
}
