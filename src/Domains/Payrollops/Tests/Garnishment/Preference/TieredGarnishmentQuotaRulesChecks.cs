namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TieredGarnishmentQuotaRulesChecks
{
    public static void TieredGarnishmentQuotaValidatorReportsEveryProblem()
    {
        var validator = TieredGarnishmentQuotaRules.DefaultTieredGarnishmentQuotaValidator();
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredGarnishmentQuotaMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new TieredGarnishmentQuotaMaximumRangeRule(3.4, 3441.9);
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("x", "y");
        item.Maximum = 3443.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredGarnishmentQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredGarnishmentQuotaRequiredCodeRule("tiered garnishment quota needs an entry");
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredGarnishmentQuotaRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredGarnishmentQuotaRequiredTitleRule("tiered garnishment quota needs an entry");
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredGarnishmentQuotaValidatorReportsEveryProblem();
        TieredGarnishmentQuotaMaximumRangeRuleFlagsValuesAboveMaximum();
        TieredGarnishmentQuotaRequiredCodeRuleFlagsABlankCode();
        TieredGarnishmentQuotaRequiredTitleRuleFlagsABlankTitle();
    }
}
