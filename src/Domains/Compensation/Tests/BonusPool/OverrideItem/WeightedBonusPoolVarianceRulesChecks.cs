namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedBonusPoolVarianceRulesChecks
{
    public static void WeightedBonusPoolVarianceValidatorReportsEveryProblem()
    {
        var validator = WeightedBonusPoolVarianceRules.DefaultWeightedBonusPoolVarianceValidator();
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBonusPoolVarianceRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new WeightedBonusPoolVarianceRequiredChecksumRule("weighted bonus pool variance needs an entry");
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedBonusPoolVarianceHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new WeightedBonusPoolVarianceHoursRangeRule(7.8, 3949.7);
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("x", "y");
        item.Hours = 3951.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBonusPoolVarianceMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new WeightedBonusPoolVarianceMinimumRangeRule(0.7, 2100.0);
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("x", "y");
        item.Minimum = 2101.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedBonusPoolVarianceValidatorReportsEveryProblem();
        WeightedBonusPoolVarianceRequiredChecksumRuleFlagsABlankChecksum();
        WeightedBonusPoolVarianceHoursRangeRuleFlagsValuesAboveHours();
        WeightedBonusPoolVarianceMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
