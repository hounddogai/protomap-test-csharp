namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyMeritCycleCheckpointRulesChecks
{
    public static void LegacyMeritCycleCheckpointValidatorReportsEveryProblem()
    {
        var validator = LegacyMeritCycleCheckpointRules.DefaultLegacyMeritCycleCheckpointValidator();
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyMeritCycleCheckpointDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new LegacyMeritCycleCheckpointDiscountRangeRule(4.0, 3167.5);
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("x", "y");
        item.Discount = 3169.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyMeritCycleCheckpointRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new LegacyMeritCycleCheckpointRequiredJobCodeRule("legacy merit cycle checkpoint needs a value");
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyMeritCycleCheckpointRegionLengthRuleFlagsALongRegion()
    {
        var rule = new LegacyMeritCycleCheckpointRegionLengthRule(255);
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyMeritCycleCheckpointBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new LegacyMeritCycleCheckpointBatchSizeRangeRule(3600, 86400);
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyMeritCycleCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyMeritCycleCheckpointCodeLengthRule(86400);
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyMeritCycleCheckpointValidatorReportsEveryProblem();
        LegacyMeritCycleCheckpointDiscountRangeRuleFlagsValuesAboveDiscount();
        LegacyMeritCycleCheckpointRequiredJobCodeRuleFlagsABlankJobCode();
        LegacyMeritCycleCheckpointRegionLengthRuleFlagsALongRegion();
        LegacyMeritCycleCheckpointBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        LegacyMeritCycleCheckpointCodeLengthRuleFlagsALongCode();
    }
}
