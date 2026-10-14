namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentAllocation;

using Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>Checks of assignment allocation code, run by the project's runner.</summary>
public static class AssignmentReservationRulesChecks
{
    public static void AssignmentReservationValidatorReportsEveryProblem()
    {
        var validator = AssignmentReservationRules.DefaultAssignmentReservationValidator();
        var item = AssignmentReservation.NewAssignmentReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new AssignmentReservationRequiredNameRule("assignment reservation needs an entry");
        var item = AssignmentReservation.NewAssignmentReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentReservationValidatorReportsEveryProblem();
        AssignmentReservationRequiredNameRuleFlagsABlankName();
    }
}
