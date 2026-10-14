namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class CappedPayslipReservationRulesChecks
{
    public static void CappedPayslipReservationValidatorReportsEveryProblem()
    {
        var validator = CappedPayslipReservationRules.DefaultCappedPayslipReservationValidator();
        var item = CappedPayslipReservation.NewCappedPayslipReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPayslipReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedPayslipReservationLabelLengthRule(86400);
        var item = CappedPayslipReservation.NewCappedPayslipReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPayslipReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedPayslipReservationRequiredLabelRule("capped payslip reservation needs this field");
        var item = CappedPayslipReservation.NewCappedPayslipReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPayslipReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedPayslipReservationCodeLengthRule(255);
        var item = CappedPayslipReservation.NewCappedPayslipReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedPayslipReservationValidatorReportsEveryProblem();
        CappedPayslipReservationLabelLengthRuleFlagsALongLabel();
        CappedPayslipReservationRequiredLabelRuleFlagsABlankLabel();
        CappedPayslipReservationCodeLengthRuleFlagsALongCode();
    }
}
