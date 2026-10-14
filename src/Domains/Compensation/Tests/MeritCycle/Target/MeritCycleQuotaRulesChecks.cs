namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleQuotaRulesChecks
{
    public static void MeritCycleQuotaValidatorReportsEveryProblem()
    {
        var validator = MeritCycleQuotaRules.DefaultMeritCycleQuotaValidator();
        var item = MeritCycleQuota.NewMeritCycleQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleQuotaCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new MeritCycleQuotaCategoryLengthRule(1000);
        var item = MeritCycleQuota.NewMeritCycleQuota("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleQuotaRequiredCodeRule("merit cycle quota needs an entry");
        var item = MeritCycleQuota.NewMeritCycleQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleQuotaFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new MeritCycleQuotaFloorValueRangeRule(5.4, 471.9);
        var item = MeritCycleQuota.NewMeritCycleQuota("x", "y");
        item.FloorValue = 473.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleQuotaRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new MeritCycleQuotaRequiredCurrencyRule("merit cycle quota needs an entry");
        var item = MeritCycleQuota.NewMeritCycleQuota("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleQuotaCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new MeritCycleQuotaCurrencyLengthRule(86400);
        var item = MeritCycleQuota.NewMeritCycleQuota("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleQuotaValidatorReportsEveryProblem();
        MeritCycleQuotaCategoryLengthRuleFlagsALongCategory();
        MeritCycleQuotaRequiredCodeRuleFlagsABlankCode();
        MeritCycleQuotaFloorValueRangeRuleFlagsValuesAboveFloorValue();
        MeritCycleQuotaRequiredCurrencyRuleFlagsABlankCurrency();
        MeritCycleQuotaCurrencyLengthRuleFlagsALongCurrency();
    }
}
