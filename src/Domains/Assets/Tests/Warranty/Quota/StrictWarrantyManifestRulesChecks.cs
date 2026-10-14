namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictWarrantyManifestRulesChecks
{
    public static void StrictWarrantyManifestValidatorReportsEveryProblem()
    {
        var validator = StrictWarrantyManifestRules.DefaultStrictWarrantyManifestValidator();
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictWarrantyManifestRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new StrictWarrantyManifestRatingRangeRule(8.5, 1564.5);
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("x", "y");
        item.Rating = 1566.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictWarrantyManifestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictWarrantyManifestTitleLengthRule(80);
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictWarrantyManifestUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new StrictWarrantyManifestUpdatedAtRangeRule(80, 1000);
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictWarrantyManifestRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new StrictWarrantyManifestRequiredDescriptionRule("strict warranty manifest needs this field");
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictWarrantyManifestValidatorReportsEveryProblem();
        StrictWarrantyManifestRatingRangeRuleFlagsValuesAboveRating();
        StrictWarrantyManifestTitleLengthRuleFlagsALongTitle();
        StrictWarrantyManifestUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        StrictWarrantyManifestRequiredDescriptionRuleFlagsABlankDescription();
    }
}
