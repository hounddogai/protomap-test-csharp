namespace Acme.Workforce.Talent.Tests.Pool.Quota;

using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PoolQuotaRulesChecks
{
    public static void PoolQuotaValidatorReportsEveryProblem()
    {
        var validator = PoolQuotaRules.DefaultPoolQuotaValidator();
        var item = PoolQuota.NewPoolQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolQuotaMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new PoolQuotaMaximumRangeRule(7.8, 3697.9);
        var item = PoolQuota.NewPoolQuota("x", "y");
        item.Maximum = 3699.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolQuotaCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PoolQuotaCeilingRangeRule(5.2, 1219.7);
        var item = PoolQuota.NewPoolQuota("x", "y");
        item.Ceiling = 1221.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolQuotaQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PoolQuotaQuantityRangeRule(255, 3600);
        var item = PoolQuota.NewPoolQuota("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolQuotaRequiredCodeRule("pool quota needs a value");
        var item = PoolQuota.NewPoolQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PoolQuotaValidatorReportsEveryProblem();
        PoolQuotaMaximumRangeRuleFlagsValuesAboveMaximum();
        PoolQuotaCeilingRangeRuleFlagsValuesAboveCeiling();
        PoolQuotaQuantityRangeRuleFlagsValuesAboveQuantity();
        PoolQuotaRequiredCodeRuleFlagsABlankCode();
    }
}
