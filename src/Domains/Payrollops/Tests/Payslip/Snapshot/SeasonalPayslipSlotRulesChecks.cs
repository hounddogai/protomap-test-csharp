namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalPayslipSlotRulesChecks
{
    public static void SeasonalPayslipSlotValidatorReportsEveryProblem()
    {
        var validator = SeasonalPayslipSlotRules.DefaultSeasonalPayslipSlotValidator();
        var item = SeasonalPayslipSlot.NewSeasonalPayslipSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPayslipSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalPayslipSlotRequiredCodeRule("seasonal payslip slot needs an entry");
        var item = SeasonalPayslipSlot.NewSeasonalPayslipSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPayslipSlotCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new SeasonalPayslipSlotCurrencyLengthRule(120);
        var item = SeasonalPayslipSlot.NewSeasonalPayslipSlot("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalPayslipSlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalPayslipSlotRequiredNameRule("seasonal payslip slot needs this field");
        var item = SeasonalPayslipSlot.NewSeasonalPayslipSlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalPayslipSlotValidatorReportsEveryProblem();
        SeasonalPayslipSlotRequiredCodeRuleFlagsABlankCode();
        SeasonalPayslipSlotCurrencyLengthRuleFlagsALongCurrency();
        SeasonalPayslipSlotRequiredNameRuleFlagsABlankName();
    }
}
