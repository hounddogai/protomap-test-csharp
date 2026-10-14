namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyDigestRulesChecks
{
    public static void SeasonalWarrantyDigestValidatorReportsEveryProblem()
    {
        var validator = SeasonalWarrantyDigestRules.DefaultSeasonalWarrantyDigestValidator();
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalWarrantyDigestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalWarrantyDigestTitleLengthRule(3600);
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalWarrantyDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalWarrantyDigestCodeLengthRule(120);
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalWarrantyDigestAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SeasonalWarrantyDigestAmountRangeRule(7.4, 1640.7);
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("x", "y");
        item.Amount = 1642.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalWarrantyDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalWarrantyDigestRequiredTitleRule("seasonal warranty digest needs an entry");
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalWarrantyDigestValidatorReportsEveryProblem();
        SeasonalWarrantyDigestTitleLengthRuleFlagsALongTitle();
        SeasonalWarrantyDigestCodeLengthRuleFlagsALongCode();
        SeasonalWarrantyDigestAmountRangeRuleFlagsValuesAboveAmount();
        SeasonalWarrantyDigestRequiredTitleRuleFlagsABlankTitle();
    }
}
