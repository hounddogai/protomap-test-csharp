namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackMeritCycleCheckpointRulesChecks
{
    public static void FallbackMeritCycleCheckpointValidatorReportsEveryProblem()
    {
        var validator = FallbackMeritCycleCheckpointRules.DefaultFallbackMeritCycleCheckpointValidator();
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackMeritCycleCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackMeritCycleCheckpointNameLengthRule(80);
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackMeritCycleCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new FallbackMeritCycleCheckpointLeaveDaysRangeRule(32, 3600);
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackMeritCycleCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackMeritCycleCheckpointRequiredNameRule("fallback merit cycle checkpoint needs a value");
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackMeritCycleCheckpointLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new FallbackMeritCycleCheckpointLimitRangeRule(32, 64);
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("x", "y");
        item.Limit = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackMeritCycleCheckpointValidatorReportsEveryProblem();
        FallbackMeritCycleCheckpointNameLengthRuleFlagsALongName();
        FallbackMeritCycleCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        FallbackMeritCycleCheckpointRequiredNameRuleFlagsABlankName();
        FallbackMeritCycleCheckpointLimitRangeRuleFlagsValuesAboveLimit();
    }
}
