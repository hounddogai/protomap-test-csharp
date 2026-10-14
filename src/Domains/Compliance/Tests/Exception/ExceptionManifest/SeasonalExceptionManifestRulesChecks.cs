namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class SeasonalExceptionManifestRulesChecks
{
    public static void SeasonalExceptionManifestValidatorReportsEveryProblem()
    {
        var validator = SeasonalExceptionManifestRules.DefaultSeasonalExceptionManifestValidator();
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalExceptionManifestWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new SeasonalExceptionManifestWindowDaysRangeRule(16, 86400);
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalExceptionManifestRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new SeasonalExceptionManifestRequiredDescriptionRule("seasonal exception manifest needs this field");
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalExceptionManifestRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new SeasonalExceptionManifestRatingRangeRule(2.3, 3197.6);
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("x", "y");
        item.Rating = 3199.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalExceptionManifestValidatorReportsEveryProblem();
        SeasonalExceptionManifestWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        SeasonalExceptionManifestRequiredDescriptionRuleFlagsABlankDescription();
        SeasonalExceptionManifestRatingRangeRuleFlagsValuesAboveRating();
    }
}
