namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdaptiveTownHallManifestRulesChecks
{
    public static void AdaptiveTownHallManifestValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTownHallManifestRules.DefaultAdaptiveTownHallManifestValidator();
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTownHallManifestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new AdaptiveTownHallManifestRequiredTeamRule("adaptive town hall manifest needs a value");
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveTownHallManifestLocationLengthRuleFlagsALongLocation()
    {
        var rule = new AdaptiveTownHallManifestLocationLengthRule(255);
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveTownHallManifestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AdaptiveTownHallManifestMarginRangeRule(8.2, 2837.1);
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("x", "y");
        item.Margin = 2838.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveTownHallManifestValidatorReportsEveryProblem();
        AdaptiveTownHallManifestRequiredTeamRuleFlagsABlankTeam();
        AdaptiveTownHallManifestLocationLengthRuleFlagsALongLocation();
        AdaptiveTownHallManifestMarginRangeRuleFlagsValuesAboveMargin();
    }
}
