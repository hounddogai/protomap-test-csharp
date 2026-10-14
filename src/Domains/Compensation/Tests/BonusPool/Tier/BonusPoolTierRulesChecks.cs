namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolTierRulesChecks
{
    public static void BonusPoolTierValidatorReportsEveryProblem()
    {
        var validator = BonusPoolTierRules.DefaultBonusPoolTierValidator();
        var item = BonusPoolTier.NewBonusPoolTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolTierHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new BonusPoolTierHoursRangeRule(0.2, 2955.4);
        var item = BonusPoolTier.NewBonusPoolTier("x", "y");
        item.Hours = 2956.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BonusPoolTierRequiredLabelRule("bonus pool tier needs an entry");
        var item = BonusPoolTier.NewBonusPoolTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolTierRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new BonusPoolTierRequiredLocationRule("bonus pool tier needs a value");
        var item = BonusPoolTier.NewBonusPoolTier("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolTierLocationLengthRuleFlagsALongLocation()
    {
        var rule = new BonusPoolTierLocationLengthRule(120);
        var item = BonusPoolTier.NewBonusPoolTier("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BonusPoolTierValidatorReportsEveryProblem();
        BonusPoolTierHoursRangeRuleFlagsValuesAboveHours();
        BonusPoolTierRequiredLabelRuleFlagsABlankLabel();
        BonusPoolTierRequiredLocationRuleFlagsABlankLocation();
        BonusPoolTierLocationLengthRuleFlagsALongLocation();
    }
}
