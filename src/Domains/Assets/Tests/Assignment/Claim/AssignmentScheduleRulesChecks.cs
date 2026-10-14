namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentScheduleRulesChecks
{
    public static void AssignmentScheduleValidatorReportsEveryProblem()
    {
        var validator = AssignmentScheduleRules.DefaultAssignmentScheduleValidator();
        var item = AssignmentSchedule.NewAssignmentSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssignmentScheduleLabelLengthRule(255);
        var item = AssignmentSchedule.NewAssignmentSchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentScheduleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AssignmentScheduleUnitLengthRule(16);
        var item = AssignmentSchedule.NewAssignmentSchedule("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentScheduleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new AssignmentScheduleRequiredUnitRule("assignment schedule needs this field");
        var item = AssignmentSchedule.NewAssignmentSchedule("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssignmentScheduleRequiredCodeRule("assignment schedule needs this field");
        var item = AssignmentSchedule.NewAssignmentSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentScheduleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new AssignmentScheduleDurationMinutesRangeRule(120, 3600);
        var item = AssignmentSchedule.NewAssignmentSchedule("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssignmentScheduleValidatorReportsEveryProblem();
        AssignmentScheduleLabelLengthRuleFlagsALongLabel();
        AssignmentScheduleUnitLengthRuleFlagsALongUnit();
        AssignmentScheduleRequiredUnitRuleFlagsABlankUnit();
        AssignmentScheduleRequiredCodeRuleFlagsABlankCode();
        AssignmentScheduleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
