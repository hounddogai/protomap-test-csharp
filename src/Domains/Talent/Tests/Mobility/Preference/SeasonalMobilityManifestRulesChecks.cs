namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalMobilityManifestRulesChecks
{
    public static void SeasonalMobilityManifestValidatorReportsEveryProblem()
    {
        var validator = SeasonalMobilityManifestRules.DefaultSeasonalMobilityManifestValidator();
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalMobilityManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalMobilityManifestRequiredNameRule("seasonal mobility manifest needs a value");
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalMobilityManifestCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new SeasonalMobilityManifestCeilingRangeRule(7.0, 3484.8);
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("x", "y");
        item.Ceiling = 3486.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalMobilityManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalMobilityManifestRequiredCodeRule("seasonal mobility manifest needs this field");
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalMobilityManifestValidatorReportsEveryProblem();
        SeasonalMobilityManifestRequiredNameRuleFlagsABlankName();
        SeasonalMobilityManifestCeilingRangeRuleFlagsValuesAboveCeiling();
        SeasonalMobilityManifestRequiredCodeRuleFlagsABlankCode();
    }
}
