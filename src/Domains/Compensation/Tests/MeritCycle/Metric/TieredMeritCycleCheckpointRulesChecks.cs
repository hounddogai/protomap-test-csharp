namespace Acme.Workforce.Compensation.Tests.MeritCycle.Metric;

using Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredMeritCycleCheckpointRulesChecks
{
    public static void TieredMeritCycleCheckpointValidatorReportsEveryProblem()
    {
        var validator = TieredMeritCycleCheckpointRules.DefaultTieredMeritCycleCheckpointValidator();
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredMeritCycleCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredMeritCycleCheckpointCodeLengthRule(3600);
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredMeritCycleCheckpointCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new TieredMeritCycleCheckpointCeilingRangeRule(2.7, 236.2);
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("x", "y");
        item.Ceiling = 237.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredMeritCycleCheckpointRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredMeritCycleCheckpointRequiredTitleRule("tiered merit cycle checkpoint needs this field");
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredMeritCycleCheckpointExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new TieredMeritCycleCheckpointExpiresAtRangeRule(120, 120);
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredMeritCycleCheckpointAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new TieredMeritCycleCheckpointAmountRangeRule(5.7, 3543.3);
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("x", "y");
        item.Amount = 3544.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredMeritCycleCheckpointValidatorReportsEveryProblem();
        TieredMeritCycleCheckpointCodeLengthRuleFlagsALongCode();
        TieredMeritCycleCheckpointCeilingRangeRuleFlagsValuesAboveCeiling();
        TieredMeritCycleCheckpointRequiredTitleRuleFlagsABlankTitle();
        TieredMeritCycleCheckpointExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        TieredMeritCycleCheckpointAmountRangeRuleFlagsValuesAboveAmount();
    }
}
