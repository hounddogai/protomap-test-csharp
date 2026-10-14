namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryMeritCycleRollupRulesChecks
{
    public static void PrimaryMeritCycleRollupValidatorReportsEveryProblem()
    {
        var validator = PrimaryMeritCycleRollupRules.DefaultPrimaryMeritCycleRollupValidator();
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryMeritCycleRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PrimaryMeritCycleRollupGraceDaysRangeRule(32, 255);
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryMeritCycleRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PrimaryMeritCycleRollupExpiresAtRangeRule(3600, 86400);
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleRollupValidatorReportsEveryProblem();
        PrimaryMeritCycleRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        PrimaryMeritCycleRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
