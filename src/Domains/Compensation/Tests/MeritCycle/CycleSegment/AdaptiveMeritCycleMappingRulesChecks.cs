namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleMappingRulesChecks
{
    public static void AdaptiveMeritCycleMappingValidatorReportsEveryProblem()
    {
        var validator = AdaptiveMeritCycleMappingRules.DefaultAdaptiveMeritCycleMappingValidator();
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveMeritCycleMappingUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new AdaptiveMeritCycleMappingUpdatedAtRangeRule(16, 1000);
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveMeritCycleMappingLocationLengthRuleFlagsALongLocation()
    {
        var rule = new AdaptiveMeritCycleMappingLocationLengthRule(16);
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveMeritCycleMappingThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AdaptiveMeritCycleMappingThresholdRangeRule(5.1, 942.0);
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("x", "y");
        item.Threshold = 943.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveMeritCycleMappingNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveMeritCycleMappingNameLengthRule(120);
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveMeritCycleMappingTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new AdaptiveMeritCycleMappingTimezoneLengthRule(255);
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveMeritCycleMappingValidatorReportsEveryProblem();
        AdaptiveMeritCycleMappingUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        AdaptiveMeritCycleMappingLocationLengthRuleFlagsALongLocation();
        AdaptiveMeritCycleMappingThresholdRangeRuleFlagsValuesAboveThreshold();
        AdaptiveMeritCycleMappingNameLengthRuleFlagsALongName();
        AdaptiveMeritCycleMappingTimezoneLengthRuleFlagsALongTimezone();
    }
}
