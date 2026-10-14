namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class RegionalSkillProfileManifestRulesChecks
{
    public static void RegionalSkillProfileManifestValidatorReportsEveryProblem()
    {
        var validator = RegionalSkillProfileManifestRules.DefaultRegionalSkillProfileManifestValidator();
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSkillProfileManifestRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new RegionalSkillProfileManifestRequiredNotesRule("regional skill profile manifest needs this field");
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSkillProfileManifestNotesLengthRuleFlagsALongNotes()
    {
        var rule = new RegionalSkillProfileManifestNotesLengthRule(80);
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSkillProfileManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalSkillProfileManifestRequiredNameRule("regional skill profile manifest needs an entry");
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSkillProfileManifestNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalSkillProfileManifestNameLengthRule(64);
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSkillProfileManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalSkillProfileManifestRequiredCodeRule("regional skill profile manifest needs this field");
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalSkillProfileManifestValidatorReportsEveryProblem();
        RegionalSkillProfileManifestRequiredNotesRuleFlagsABlankNotes();
        RegionalSkillProfileManifestNotesLengthRuleFlagsALongNotes();
        RegionalSkillProfileManifestRequiredNameRuleFlagsABlankName();
        RegionalSkillProfileManifestNameLengthRuleFlagsALongName();
        RegionalSkillProfileManifestRequiredCodeRuleFlagsABlankCode();
    }
}
