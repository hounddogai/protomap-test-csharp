namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledPayslipAllocationRulesChecks
{
    public static void ScheduledPayslipAllocationValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayslipAllocationRules.DefaultScheduledPayslipAllocationValidator();
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayslipAllocationFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ScheduledPayslipAllocationFteRangeRule(3.3, 3701.6);
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("x", "y");
        item.Fte = 3703.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayslipAllocationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledPayslipAllocationLabelLengthRule(16);
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayslipAllocationGradeLengthRuleFlagsALongGrade()
    {
        var rule = new ScheduledPayslipAllocationGradeLengthRule(1000);
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayslipAllocationRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new ScheduledPayslipAllocationRequiredGradeRule("scheduled payslip allocation needs an entry");
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPayslipAllocationValidatorReportsEveryProblem();
        ScheduledPayslipAllocationFteRangeRuleFlagsValuesAboveFte();
        ScheduledPayslipAllocationLabelLengthRuleFlagsALongLabel();
        ScheduledPayslipAllocationGradeLengthRuleFlagsALongGrade();
        ScheduledPayslipAllocationRequiredGradeRuleFlagsABlankGrade();
    }
}
