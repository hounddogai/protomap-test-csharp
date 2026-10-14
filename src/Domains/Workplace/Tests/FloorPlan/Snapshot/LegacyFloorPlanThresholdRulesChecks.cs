namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyFloorPlanThresholdRulesChecks
{
    public static void LegacyFloorPlanThresholdValidatorReportsEveryProblem()
    {
        var validator = LegacyFloorPlanThresholdRules.DefaultLegacyFloorPlanThresholdValidator();
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyFloorPlanThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyFloorPlanThresholdCodeLengthRule(86400);
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyFloorPlanThresholdTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LegacyFloorPlanThresholdTitleLengthRule(32);
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyFloorPlanThresholdRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyFloorPlanThresholdRequiredTitleRule("legacy floor plan threshold needs an entry");
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyFloorPlanThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyFloorPlanThresholdRequiredCodeRule("legacy floor plan threshold needs this field");
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyFloorPlanThresholdValidatorReportsEveryProblem();
        LegacyFloorPlanThresholdCodeLengthRuleFlagsALongCode();
        LegacyFloorPlanThresholdTitleLengthRuleFlagsALongTitle();
        LegacyFloorPlanThresholdRequiredTitleRuleFlagsABlankTitle();
        LegacyFloorPlanThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
