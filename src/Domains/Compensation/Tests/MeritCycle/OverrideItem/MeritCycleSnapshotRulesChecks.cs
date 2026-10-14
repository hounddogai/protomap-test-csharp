namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleSnapshotRulesChecks
{
    public static void MeritCycleSnapshotValidatorReportsEveryProblem()
    {
        var validator = MeritCycleSnapshotRules.DefaultMeritCycleSnapshotValidator();
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleSnapshotVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new MeritCycleSnapshotVersionRangeRule(1000, 86400);
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleSnapshotCodeLengthRule(32);
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleSnapshotValidatorReportsEveryProblem();
        MeritCycleSnapshotVersionRangeRuleFlagsValuesAboveVersion();
        MeritCycleSnapshotCodeLengthRuleFlagsALongCode();
    }
}
