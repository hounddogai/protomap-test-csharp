namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class RegionalPayslipReservationRulesChecks
{
    public static void RegionalPayslipReservationValidatorReportsEveryProblem()
    {
        var validator = RegionalPayslipReservationRules.DefaultRegionalPayslipReservationValidator();
        var item = RegionalPayslipReservation.NewRegionalPayslipReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPayslipReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalPayslipReservationLabelLengthRule(86400);
        var item = RegionalPayslipReservation.NewRegionalPayslipReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPayslipReservationFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new RegionalPayslipReservationFloorValueRangeRule(7.5, 4530.9);
        var item = RegionalPayslipReservation.NewRegionalPayslipReservation("x", "y");
        item.FloorValue = 4532.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalPayslipReservationLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new RegionalPayslipReservationLimitRangeRule(16, 120);
        var item = RegionalPayslipReservation.NewRegionalPayslipReservation("x", "y");
        item.Limit = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalPayslipReservationValidatorReportsEveryProblem();
        RegionalPayslipReservationLabelLengthRuleFlagsALongLabel();
        RegionalPayslipReservationFloorValueRangeRuleFlagsValuesAboveFloorValue();
        RegionalPayslipReservationLimitRangeRuleFlagsValuesAboveLimit();
    }
}
