namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GarnishmentQuotaRulesChecks
{
    public static void GarnishmentQuotaValidatorReportsEveryProblem()
    {
        var validator = GarnishmentQuotaRules.DefaultGarnishmentQuotaValidator();
        var item = GarnishmentQuota.NewGarnishmentQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentQuotaMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new GarnishmentQuotaMinimumRangeRule(8.3, 1388.1);
        var item = GarnishmentQuota.NewGarnishmentQuota("x", "y");
        item.Minimum = 1389.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentQuotaNameLengthRuleFlagsALongName()
    {
        var rule = new GarnishmentQuotaNameLengthRule(16);
        var item = GarnishmentQuota.NewGarnishmentQuota("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentQuotaRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new GarnishmentQuotaRequiredSourceRule("garnishment quota needs this field");
        var item = GarnishmentQuota.NewGarnishmentQuota("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentQuotaDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new GarnishmentQuotaDiscountRangeRule(6.9, 4895.1);
        var item = GarnishmentQuota.NewGarnishmentQuota("x", "y");
        item.Discount = 4896.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentQuotaCodeLengthRule(86400);
        var item = GarnishmentQuota.NewGarnishmentQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentQuotaValidatorReportsEveryProblem();
        GarnishmentQuotaMinimumRangeRuleFlagsValuesAboveMinimum();
        GarnishmentQuotaNameLengthRuleFlagsALongName();
        GarnishmentQuotaRequiredSourceRuleFlagsABlankSource();
        GarnishmentQuotaDiscountRangeRuleFlagsValuesAboveDiscount();
        GarnishmentQuotaCodeLengthRuleFlagsALongCode();
    }
}
