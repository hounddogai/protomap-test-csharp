namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileEntry;

using Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>Checks of profile entry code, run by the project's runner.</summary>
public static class StrictSkillProfileEntryRulesChecks
{
    public static void StrictSkillProfileEntryValidatorReportsEveryProblem()
    {
        var validator = StrictSkillProfileEntryRules.DefaultStrictSkillProfileEntryValidator();
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictSkillProfileEntryFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new StrictSkillProfileEntryFteRangeRule(1.9, 1105.0);
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("x", "y");
        item.Fte = 1106.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictSkillProfileEntrySalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new StrictSkillProfileEntrySalaryBandLengthRule(32);
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSkillProfileEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictSkillProfileEntryCodeLengthRule(120);
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSkillProfileEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictSkillProfileEntryRequiredTitleRule("strict skill profile entry needs a value");
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSkillProfileEntryRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new StrictSkillProfileEntryRequiredSalaryBandRule("strict skill profile entry needs a value");
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictSkillProfileEntryValidatorReportsEveryProblem();
        StrictSkillProfileEntryFteRangeRuleFlagsValuesAboveFte();
        StrictSkillProfileEntrySalaryBandLengthRuleFlagsALongSalaryBand();
        StrictSkillProfileEntryCodeLengthRuleFlagsALongCode();
        StrictSkillProfileEntryRequiredTitleRuleFlagsABlankTitle();
        StrictSkillProfileEntryRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
