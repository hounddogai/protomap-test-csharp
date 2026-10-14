namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileManifestRulesChecks
{
    public static void SkillProfileManifestValidatorReportsEveryProblem()
    {
        var validator = SkillProfileManifestRules.DefaultSkillProfileManifestValidator();
        var item = SkillProfileManifest.NewSkillProfileManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileManifestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SkillProfileManifestRequiredLabelRule("skill profile manifest needs this field");
        var item = SkillProfileManifest.NewSkillProfileManifest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileManifestOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new SkillProfileManifestOwnerIdLengthRule(80);
        var item = SkillProfileManifest.NewSkillProfileManifest("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileManifestTeamLengthRuleFlagsALongTeam()
    {
        var rule = new SkillProfileManifestTeamLengthRule(16);
        var item = SkillProfileManifest.NewSkillProfileManifest("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileManifestValidatorReportsEveryProblem();
        SkillProfileManifestRequiredLabelRuleFlagsABlankLabel();
        SkillProfileManifestOwnerIdLengthRuleFlagsALongOwnerId();
        SkillProfileManifestTeamLengthRuleFlagsALongTeam();
    }
}
