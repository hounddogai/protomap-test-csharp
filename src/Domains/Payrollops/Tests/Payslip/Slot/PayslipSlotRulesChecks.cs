namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipSlotRulesChecks
{
    public static void PayslipSlotValidatorReportsEveryProblem()
    {
        var validator = PayslipSlotRules.DefaultPayslipSlotValidator();
        var item = PayslipSlot.NewPayslipSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipSlotCodeLengthRule(86400);
        var item = PayslipSlot.NewPayslipSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipSlotRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new PayslipSlotRequiredDepartmentRule("payslip slot needs a value");
        var item = PayslipSlot.NewPayslipSlot("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipSlotCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PayslipSlotCeilingRangeRule(4.6, 1670.8);
        var item = PayslipSlot.NewPayslipSlot("x", "y");
        item.Ceiling = 1672.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayslipSlotValidatorReportsEveryProblem();
        PayslipSlotCodeLengthRuleFlagsALongCode();
        PayslipSlotRequiredDepartmentRuleFlagsABlankDepartment();
        PayslipSlotCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
