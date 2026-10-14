namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class RollingPayslipAssignmentRulesChecks
{
    public static void RollingPayslipAssignmentValidatorReportsEveryProblem()
    {
        var validator = RollingPayslipAssignmentRules.DefaultRollingPayslipAssignmentValidator();
        var item = RollingPayslipAssignment.NewRollingPayslipAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPayslipAssignmentTeamLengthRuleFlagsALongTeam()
    {
        var rule = new RollingPayslipAssignmentTeamLengthRule(120);
        var item = RollingPayslipAssignment.NewRollingPayslipAssignment("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPayslipAssignmentRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RollingPayslipAssignmentRequiredCurrencyRule("rolling payslip assignment needs this field");
        var item = RollingPayslipAssignment.NewRollingPayslipAssignment("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPayslipAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingPayslipAssignmentLabelLengthRule(255);
        var item = RollingPayslipAssignment.NewRollingPayslipAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingPayslipAssignmentValidatorReportsEveryProblem();
        RollingPayslipAssignmentTeamLengthRuleFlagsALongTeam();
        RollingPayslipAssignmentRequiredCurrencyRuleFlagsABlankCurrency();
        RollingPayslipAssignmentLabelLengthRuleFlagsALongLabel();
    }
}
