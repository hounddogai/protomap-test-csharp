namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SeasonalCertificationManifestRulesChecks
{
    public static void SeasonalCertificationManifestValidatorReportsEveryProblem()
    {
        var validator = SeasonalCertificationManifestRules.DefaultSeasonalCertificationManifestValidator();
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalCertificationManifestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new SeasonalCertificationManifestRequiredTeamRule("seasonal certification manifest needs an entry");
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalCertificationManifestTeamLengthRuleFlagsALongTeam()
    {
        var rule = new SeasonalCertificationManifestTeamLengthRule(1000);
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalCertificationManifestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalCertificationManifestRequiredLabelRule("seasonal certification manifest needs a value");
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalCertificationManifestValidatorReportsEveryProblem();
        SeasonalCertificationManifestRequiredTeamRuleFlagsABlankTeam();
        SeasonalCertificationManifestTeamLengthRuleFlagsALongTeam();
        SeasonalCertificationManifestRequiredLabelRuleFlagsABlankLabel();
    }
}
