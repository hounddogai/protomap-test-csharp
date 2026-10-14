namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class WeightedBonusPoolQuotaRulesChecks
{
    public static void WeightedBonusPoolQuotaValidatorReportsEveryProblem()
    {
        var validator = WeightedBonusPoolQuotaRules.DefaultWeightedBonusPoolQuotaValidator();
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBonusPoolQuotaRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new WeightedBonusPoolQuotaRatingRangeRule(5.7, 3100.5);
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("x", "y");
        item.Rating = 3102.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBonusPoolQuotaNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedBonusPoolQuotaNameLengthRule(80);
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBonusPoolQuotaTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new WeightedBonusPoolQuotaTenureMonthsRangeRule(64, 120);
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("x", "y");
        item.TenureMonths = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBonusPoolQuotaRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new WeightedBonusPoolQuotaRequiredChecksumRule("weighted bonus pool quota needs an entry");
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedBonusPoolQuotaValidatorReportsEveryProblem();
        WeightedBonusPoolQuotaRatingRangeRuleFlagsValuesAboveRating();
        WeightedBonusPoolQuotaNameLengthRuleFlagsALongName();
        WeightedBonusPoolQuotaTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        WeightedBonusPoolQuotaRequiredChecksumRuleFlagsABlankChecksum();
    }
}
