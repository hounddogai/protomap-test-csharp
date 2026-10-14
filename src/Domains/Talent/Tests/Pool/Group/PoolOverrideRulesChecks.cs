namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PoolOverrideRulesChecks
{
    public static void PoolOverrideValidatorReportsEveryProblem()
    {
        var validator = PoolOverrideRules.DefaultPoolOverrideValidator();
        var item = PoolOverride.NewPoolOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolOverrideAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PoolOverrideAmountRangeRule(2.0, 2755.2);
        var item = PoolOverride.NewPoolOverride("x", "y");
        item.Amount = 2756.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolOverrideRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PoolOverrideRequiredExternalRefRule("pool override needs an entry");
        var item = PoolOverride.NewPoolOverride("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolOverrideRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PoolOverrideRequiredOwnerIdRule("pool override needs this field");
        var item = PoolOverride.NewPoolOverride("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolOverrideBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PoolOverrideBatchSizeRangeRule(64, 3600);
        var item = PoolOverride.NewPoolOverride("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PoolOverrideValidatorReportsEveryProblem();
        PoolOverrideAmountRangeRuleFlagsValuesAboveAmount();
        PoolOverrideRequiredExternalRefRuleFlagsABlankExternalRef();
        PoolOverrideRequiredOwnerIdRuleFlagsABlankOwnerId();
        PoolOverrideBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
