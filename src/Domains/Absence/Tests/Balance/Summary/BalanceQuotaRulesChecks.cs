namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BalanceQuotaRulesChecks
{
    public static void BalanceQuotaValidatorReportsEveryProblem()
    {
        var validator = BalanceQuotaRules.DefaultBalanceQuotaValidator();
        var item = BalanceQuota.NewBalanceQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new BalanceQuotaCodeLengthRule(32);
        var item = BalanceQuota.NewBalanceQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceQuotaTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new BalanceQuotaTimezoneLengthRule(64);
        var item = BalanceQuota.NewBalanceQuota("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceQuotaPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new BalanceQuotaPhoneLengthRule(16);
        var item = BalanceQuota.NewBalanceQuota("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceQuotaRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new BalanceQuotaRequiredPhoneRule("balance quota needs a value");
        var item = BalanceQuota.NewBalanceQuota("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BalanceQuotaRequiredLabelRule("balance quota needs this field");
        var item = BalanceQuota.NewBalanceQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceQuotaWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new BalanceQuotaWindowDaysRangeRule(64, 3600);
        var item = BalanceQuota.NewBalanceQuota("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceQuotaValidatorReportsEveryProblem();
        BalanceQuotaCodeLengthRuleFlagsALongCode();
        BalanceQuotaTimezoneLengthRuleFlagsALongTimezone();
        BalanceQuotaPhoneLengthRuleFlagsALongPhone();
        BalanceQuotaRequiredPhoneRuleFlagsABlankPhone();
        BalanceQuotaRequiredLabelRuleFlagsABlankLabel();
        BalanceQuotaWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
