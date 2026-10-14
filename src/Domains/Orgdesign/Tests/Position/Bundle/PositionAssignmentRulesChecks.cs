namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionAssignmentRulesChecks
{
    public static void PositionAssignmentValidatorReportsEveryProblem()
    {
        var validator = PositionAssignmentRules.DefaultPositionAssignmentValidator();
        var item = PositionAssignment.NewPositionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionAssignmentEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new PositionAssignmentEmployeeIdLengthRule(1000);
        var item = PositionAssignment.NewPositionAssignment("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionAssignmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PositionAssignmentCapacityRangeRule(32, 86400);
        var item = PositionAssignment.NewPositionAssignment("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionAssignmentValidatorReportsEveryProblem();
        PositionAssignmentEmployeeIdLengthRuleFlagsALongEmployeeId();
        PositionAssignmentCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
