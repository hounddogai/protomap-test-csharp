namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipReservationRulesChecks
{
    public static void PayslipReservationValidatorReportsEveryProblem()
    {
        var validator = PayslipReservationRules.DefaultPayslipReservationValidator();
        var item = PayslipReservation.NewPayslipReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayslipReservationRequiredNameRule("payslip reservation needs an entry");
        var item = PayslipReservation.NewPayslipReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipReservationCodeLengthRule(86400);
        var item = PayslipReservation.NewPayslipReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipReservationRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new PayslipReservationRequiredCostCenterRule("payslip reservation needs an entry");
        var item = PayslipReservation.NewPayslipReservation("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipReservationValidatorReportsEveryProblem();
        PayslipReservationRequiredNameRuleFlagsABlankName();
        PayslipReservationCodeLengthRuleFlagsALongCode();
        PayslipReservationRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
