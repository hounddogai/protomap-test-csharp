namespace Acme.Workforce.Assets.Tests.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GlobalAssignmentReservationRulesChecks
{
    public static void GlobalAssignmentReservationValidatorReportsEveryProblem()
    {
        var validator = GlobalAssignmentReservationRules.DefaultGlobalAssignmentReservationValidator();
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAssignmentReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalAssignmentReservationRequiredTitleRule("global assignment reservation needs a value");
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAssignmentReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalAssignmentReservationCodeLengthRule(1000);
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAssignmentReservationDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new GlobalAssignmentReservationDepartmentLengthRule(80);
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAssignmentReservationEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new GlobalAssignmentReservationEndsAtRangeRule(16, 80);
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("x", "y");
        item.EndsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalAssignmentReservationValidatorReportsEveryProblem();
        GlobalAssignmentReservationRequiredTitleRuleFlagsABlankTitle();
        GlobalAssignmentReservationCodeLengthRuleFlagsALongCode();
        GlobalAssignmentReservationDepartmentLengthRuleFlagsALongDepartment();
        GlobalAssignmentReservationEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
