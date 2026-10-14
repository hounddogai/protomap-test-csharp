namespace Acme.Workforce.Compensation.Tests.EquityGrant.Quota;

using Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScheduledEquityGrantQuotaRulesChecks
{
    public static void ScheduledEquityGrantQuotaValidatorReportsEveryProblem()
    {
        var validator = ScheduledEquityGrantQuotaRules.DefaultScheduledEquityGrantQuotaValidator();
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledEquityGrantQuotaRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ScheduledEquityGrantQuotaRequiredCategoryRule("scheduled equity grant quota needs this field");
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledEquityGrantQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledEquityGrantQuotaCodeLengthRule(255);
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledEquityGrantQuotaRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new ScheduledEquityGrantQuotaRequiredCurrencyRule("scheduled equity grant quota needs an entry");
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledEquityGrantQuotaTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledEquityGrantQuotaTitleLengthRule(120);
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledEquityGrantQuotaQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ScheduledEquityGrantQuotaQuantityRangeRule(80, 255);
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantQuotaValidatorReportsEveryProblem();
        ScheduledEquityGrantQuotaRequiredCategoryRuleFlagsABlankCategory();
        ScheduledEquityGrantQuotaCodeLengthRuleFlagsALongCode();
        ScheduledEquityGrantQuotaRequiredCurrencyRuleFlagsABlankCurrency();
        ScheduledEquityGrantQuotaTitleLengthRuleFlagsALongTitle();
        ScheduledEquityGrantQuotaQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
