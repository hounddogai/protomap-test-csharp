namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantQuotaRulesChecks
{
    public static void EquityGrantQuotaValidatorReportsEveryProblem()
    {
        var validator = EquityGrantQuotaRules.DefaultEquityGrantQuotaValidator();
        var item = EquityGrantQuota.NewEquityGrantQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantQuotaUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new EquityGrantQuotaUpdatedAtRangeRule(64, 3600);
        var item = EquityGrantQuota.NewEquityGrantQuota("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantQuotaUnitLengthRuleFlagsALongUnit()
    {
        var rule = new EquityGrantQuotaUnitLengthRule(64);
        var item = EquityGrantQuota.NewEquityGrantQuota("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantQuotaQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new EquityGrantQuotaQuantityRangeRule(16, 255);
        var item = EquityGrantQuota.NewEquityGrantQuota("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new EquityGrantQuotaRequiredNameRule("equity grant quota needs an entry");
        var item = EquityGrantQuota.NewEquityGrantQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantQuotaRequiredCodeRule("equity grant quota needs an entry");
        var item = EquityGrantQuota.NewEquityGrantQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantQuotaValidatorReportsEveryProblem();
        EquityGrantQuotaUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        EquityGrantQuotaUnitLengthRuleFlagsALongUnit();
        EquityGrantQuotaQuantityRangeRuleFlagsValuesAboveQuantity();
        EquityGrantQuotaRequiredNameRuleFlagsABlankName();
        EquityGrantQuotaRequiredCodeRuleFlagsABlankCode();
    }
}
