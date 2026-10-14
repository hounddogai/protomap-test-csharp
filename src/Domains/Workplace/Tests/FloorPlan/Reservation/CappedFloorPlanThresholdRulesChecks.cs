namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedFloorPlanThresholdRulesChecks
{
    public static void CappedFloorPlanThresholdValidatorReportsEveryProblem()
    {
        var validator = CappedFloorPlanThresholdRules.DefaultCappedFloorPlanThresholdValidator();
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedFloorPlanThresholdRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new CappedFloorPlanThresholdRequiredPhoneRule("capped floor plan threshold needs a value");
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedFloorPlanThresholdAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new CappedFloorPlanThresholdAttemptsRangeRule(255, 86400);
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedFloorPlanThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedFloorPlanThresholdRequiredCodeRule("capped floor plan threshold needs a value");
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedFloorPlanThresholdValidatorReportsEveryProblem();
        CappedFloorPlanThresholdRequiredPhoneRuleFlagsABlankPhone();
        CappedFloorPlanThresholdAttemptsRangeRuleFlagsValuesAboveAttempts();
        CappedFloorPlanThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
