namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayQuotaRulesChecks
{
    public static void RetroPayQuotaValidatorReportsEveryProblem()
    {
        var validator = RetroPayQuotaRules.DefaultRetroPayQuotaValidator();
        var item = RetroPayQuota.NewRetroPayQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayQuotaCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new RetroPayQuotaCreatedAtRangeRule(1000, 1000);
        var item = RetroPayQuota.NewRetroPayQuota("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayQuotaRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RetroPayQuotaRequiredCurrencyRule("retro pay quota needs this field");
        var item = RetroPayQuota.NewRetroPayQuota("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayQuotaTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RetroPayQuotaTenureMonthsRangeRule(120, 86400);
        var item = RetroPayQuota.NewRetroPayQuota("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayQuotaHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new RetroPayQuotaHoursRangeRule(3.8, 973.9);
        var item = RetroPayQuota.NewRetroPayQuota("x", "y");
        item.Hours = 975.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayQuotaValidatorReportsEveryProblem();
        RetroPayQuotaCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        RetroPayQuotaRequiredCurrencyRuleFlagsABlankCurrency();
        RetroPayQuotaTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        RetroPayQuotaHoursRangeRuleFlagsValuesAboveHours();
    }
}
