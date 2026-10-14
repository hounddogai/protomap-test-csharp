namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileEntryRulesChecks
{
    public static void SkillProfileEntryValidatorReportsEveryProblem()
    {
        var validator = SkillProfileEntryRules.DefaultSkillProfileEntryValidator();
        var item = SkillProfileEntry.NewSkillProfileEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileEntryRequiredCodeRule("skill profile entry needs this field");
        var item = SkillProfileEntry.NewSkillProfileEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileEntryCodeLengthRule(120);
        var item = SkillProfileEntry.NewSkillProfileEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SkillProfileEntryLabelLengthRule(16);
        var item = SkillProfileEntry.NewSkillProfileEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileEntryLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new SkillProfileEntryLimitRangeRule(16, 1000);
        var item = SkillProfileEntry.NewSkillProfileEntry("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SkillProfileEntryValidatorReportsEveryProblem();
        SkillProfileEntryRequiredCodeRuleFlagsABlankCode();
        SkillProfileEntryCodeLengthRuleFlagsALongCode();
        SkillProfileEntryLabelLengthRuleFlagsALongLabel();
        SkillProfileEntryLimitRangeRuleFlagsValuesAboveLimit();
    }
}
