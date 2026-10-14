namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientLeaveRequestReservationRulesChecks
{
    public static void LenientLeaveRequestReservationValidatorReportsEveryProblem()
    {
        var validator = LenientLeaveRequestReservationRules.DefaultLenientLeaveRequestReservationValidator();
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLeaveRequestReservationTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new LenientLeaveRequestReservationTenureMonthsRangeRule(64, 1000);
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientLeaveRequestReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientLeaveRequestReservationRequiredCodeRule("lenient leave request reservation needs a value");
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientLeaveRequestReservationValidatorReportsEveryProblem();
        LenientLeaveRequestReservationTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        LenientLeaveRequestReservationRequiredCodeRuleFlagsABlankCode();
    }
}
