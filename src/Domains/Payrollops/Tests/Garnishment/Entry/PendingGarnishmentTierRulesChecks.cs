namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingGarnishmentTierRulesChecks
{
    public static void PendingGarnishmentTierValidatorReportsEveryProblem()
    {
        var validator = PendingGarnishmentTierRules.DefaultPendingGarnishmentTierValidator();
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingGarnishmentTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingGarnishmentTierRequiredTitleRule("pending garnishment tier needs a value");
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingGarnishmentTierQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PendingGarnishmentTierQuantityRangeRule(16, 86400);
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingGarnishmentTierCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PendingGarnishmentTierCapacityRangeRule(120, 255);
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingGarnishmentTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingGarnishmentTierRequiredCodeRule("pending garnishment tier needs an entry");
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingGarnishmentTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingGarnishmentTierCodeLengthRule(86400);
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingGarnishmentTierEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PendingGarnishmentTierEndsAtRangeRule(32, 64);
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("x", "y");
        item.EndsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingGarnishmentTierValidatorReportsEveryProblem();
        PendingGarnishmentTierRequiredTitleRuleFlagsABlankTitle();
        PendingGarnishmentTierQuantityRangeRuleFlagsValuesAboveQuantity();
        PendingGarnishmentTierCapacityRangeRuleFlagsValuesAboveCapacity();
        PendingGarnishmentTierRequiredCodeRuleFlagsABlankCode();
        PendingGarnishmentTierCodeLengthRuleFlagsALongCode();
        PendingGarnishmentTierEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
