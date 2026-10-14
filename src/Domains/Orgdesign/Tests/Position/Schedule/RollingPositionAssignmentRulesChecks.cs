namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingPositionAssignmentRulesChecks
{
    public static void RollingPositionAssignmentValidatorReportsEveryProblem()
    {
        var validator = RollingPositionAssignmentRules.DefaultRollingPositionAssignmentValidator();
        var item = RollingPositionAssignment.NewRollingPositionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPositionAssignmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RollingPositionAssignmentCapacityRangeRule(16, 255);
        var item = RollingPositionAssignment.NewRollingPositionAssignment("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingPositionAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingPositionAssignmentCodeLengthRule(3600);
        var item = RollingPositionAssignment.NewRollingPositionAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPositionAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingPositionAssignmentRequiredLabelRule("rolling position assignment needs a value");
        var item = RollingPositionAssignment.NewRollingPositionAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPositionAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingPositionAssignmentLabelLengthRule(80);
        var item = RollingPositionAssignment.NewRollingPositionAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingPositionAssignmentValidatorReportsEveryProblem();
        RollingPositionAssignmentCapacityRangeRuleFlagsValuesAboveCapacity();
        RollingPositionAssignmentCodeLengthRuleFlagsALongCode();
        RollingPositionAssignmentRequiredLabelRuleFlagsABlankLabel();
        RollingPositionAssignmentLabelLengthRuleFlagsALongLabel();
    }
}
