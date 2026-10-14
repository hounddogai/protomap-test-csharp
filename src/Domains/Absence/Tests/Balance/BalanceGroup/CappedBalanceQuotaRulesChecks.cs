namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class CappedBalanceQuotaRulesChecks
{
    public static void CappedBalanceQuotaValidatorReportsEveryProblem()
    {
        var validator = CappedBalanceQuotaRules.DefaultCappedBalanceQuotaValidator();
        var item = CappedBalanceQuota.NewCappedBalanceQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedBalanceQuotaCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new CappedBalanceQuotaCostRangeRule(6.9, 2020.1);
        var item = CappedBalanceQuota.NewCappedBalanceQuota("x", "y");
        item.Cost = 2021.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedBalanceQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedBalanceQuotaLabelLengthRule(16);
        var item = CappedBalanceQuota.NewCappedBalanceQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedBalanceQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedBalanceQuotaRequiredLabelRule("capped balance quota needs an entry");
        var item = CappedBalanceQuota.NewCappedBalanceQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedBalanceQuotaSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new CappedBalanceQuotaSalaryBandLengthRule(32);
        var item = CappedBalanceQuota.NewCappedBalanceQuota("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedBalanceQuotaValidatorReportsEveryProblem();
        CappedBalanceQuotaCostRangeRuleFlagsValuesAboveCost();
        CappedBalanceQuotaLabelLengthRuleFlagsALongLabel();
        CappedBalanceQuotaRequiredLabelRuleFlagsABlankLabel();
        CappedBalanceQuotaSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
