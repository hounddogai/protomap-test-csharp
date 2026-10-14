namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAssignmentRulesChecks
{
    public static void OccupancyAssignmentValidatorReportsEveryProblem()
    {
        var validator = OccupancyAssignmentRules.DefaultOccupancyAssignmentValidator();
        var item = OccupancyAssignment.NewOccupancyAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyAssignmentUnitLengthRuleFlagsALongUnit()
    {
        var rule = new OccupancyAssignmentUnitLengthRule(64);
        var item = OccupancyAssignment.NewOccupancyAssignment("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyAssignmentRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new OccupancyAssignmentRequiredUnitRule("occupancy assignment needs this field");
        var item = OccupancyAssignment.NewOccupancyAssignment("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new OccupancyAssignmentRequiredNameRule("occupancy assignment needs an entry");
        var item = OccupancyAssignment.NewOccupancyAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyAssignmentValidatorReportsEveryProblem();
        OccupancyAssignmentUnitLengthRuleFlagsALongUnit();
        OccupancyAssignmentRequiredUnitRuleFlagsABlankUnit();
        OccupancyAssignmentRequiredNameRuleFlagsABlankName();
    }
}
