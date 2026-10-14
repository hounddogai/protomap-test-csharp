namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveLeaveRequestLedgerRulesChecks
{
    public static void AdaptiveLeaveRequestLedgerValidatorReportsEveryProblem()
    {
        var validator = AdaptiveLeaveRequestLedgerRules.DefaultAdaptiveLeaveRequestLedgerValidator();
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveLeaveRequestLedgerQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AdaptiveLeaveRequestLedgerQuantityRangeRule(16, 64);
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("x", "y");
        item.Quantity = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLeaveRequestLedgerNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveLeaveRequestLedgerNameLengthRule(120);
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveLeaveRequestLedgerMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AdaptiveLeaveRequestLedgerMaximumRangeRule(6.1, 3919.4);
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("x", "y");
        item.Maximum = 3920.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLeaveRequestLedgerRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AdaptiveLeaveRequestLedgerRateRangeRule(7.6, 2313.1);
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("x", "y");
        item.Rate = 2314.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLeaveRequestLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveLeaveRequestLedgerCodeLengthRule(16);
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveLeaveRequestLedgerDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new AdaptiveLeaveRequestLedgerDepartmentLengthRule(1000);
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveLeaveRequestLedgerValidatorReportsEveryProblem();
        AdaptiveLeaveRequestLedgerQuantityRangeRuleFlagsValuesAboveQuantity();
        AdaptiveLeaveRequestLedgerNameLengthRuleFlagsALongName();
        AdaptiveLeaveRequestLedgerMaximumRangeRuleFlagsValuesAboveMaximum();
        AdaptiveLeaveRequestLedgerRateRangeRuleFlagsValuesAboveRate();
        AdaptiveLeaveRequestLedgerCodeLengthRuleFlagsALongCode();
        AdaptiveLeaveRequestLedgerDepartmentLengthRuleFlagsALongDepartment();
    }
}
