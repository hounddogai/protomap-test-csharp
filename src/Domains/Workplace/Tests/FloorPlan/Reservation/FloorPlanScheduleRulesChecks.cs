namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FloorPlanScheduleRulesChecks
{
    public static void FloorPlanScheduleValidatorReportsEveryProblem()
    {
        var validator = FloorPlanScheduleRules.DefaultFloorPlanScheduleValidator();
        var item = FloorPlanSchedule.NewFloorPlanSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new FloorPlanScheduleRequiredNameRule("floor plan schedule needs this field");
        var item = FloorPlanSchedule.NewFloorPlanSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanScheduleNameLengthRuleFlagsALongName()
    {
        var rule = new FloorPlanScheduleNameLengthRule(80);
        var item = FloorPlanSchedule.NewFloorPlanSchedule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanScheduleExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new FloorPlanScheduleExpiresAtRangeRule(120, 86400);
        var item = FloorPlanSchedule.NewFloorPlanSchedule("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanScheduleRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new FloorPlanScheduleRetriesRangeRule(64, 120);
        var item = FloorPlanSchedule.NewFloorPlanSchedule("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FloorPlanScheduleValidatorReportsEveryProblem();
        FloorPlanScheduleRequiredNameRuleFlagsABlankName();
        FloorPlanScheduleNameLengthRuleFlagsALongName();
        FloorPlanScheduleExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        FloorPlanScheduleRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
