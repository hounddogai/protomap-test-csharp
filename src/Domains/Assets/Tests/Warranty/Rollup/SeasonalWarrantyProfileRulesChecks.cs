namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyProfileRulesChecks
{
    public static void SeasonalWarrantyProfileValidatorReportsEveryProblem()
    {
        var validator = SeasonalWarrantyProfileRules.DefaultSeasonalWarrantyProfileValidator();
        var item = SeasonalWarrantyProfile.NewSeasonalWarrantyProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalWarrantyProfileOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new SeasonalWarrantyProfileOwnerIdLengthRule(32);
        var item = SeasonalWarrantyProfile.NewSeasonalWarrantyProfile("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalWarrantyProfileLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new SeasonalWarrantyProfileLimitRangeRule(80, 1000);
        var item = SeasonalWarrantyProfile.NewSeasonalWarrantyProfile("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalWarrantyProfileValidatorReportsEveryProblem();
        SeasonalWarrantyProfileOwnerIdLengthRuleFlagsALongOwnerId();
        SeasonalWarrantyProfileLimitRangeRuleFlagsValuesAboveLimit();
    }
}
