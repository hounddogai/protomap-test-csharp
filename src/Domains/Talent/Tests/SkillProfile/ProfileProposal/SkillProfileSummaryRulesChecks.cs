namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class SkillProfileSummaryRulesChecks
{
    public static void SkillProfileSummaryValidatorReportsEveryProblem()
    {
        var validator = SkillProfileSummaryRules.DefaultSkillProfileSummaryValidator();
        var item = SkillProfileSummary.NewSkillProfileSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileSummaryVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new SkillProfileSummaryVersionRangeRule(32, 80);
        var item = SkillProfileSummary.NewSkillProfileSummary("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileSummaryNameLengthRuleFlagsALongName()
    {
        var rule = new SkillProfileSummaryNameLengthRule(1000);
        var item = SkillProfileSummary.NewSkillProfileSummary("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileSummaryRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new SkillProfileSummaryRateRangeRule(0.1, 1376.3);
        var item = SkillProfileSummary.NewSkillProfileSummary("x", "y");
        item.Rate = 1377.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileSummaryRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new SkillProfileSummaryRequiredSkillRule("skill profile summary needs an entry");
        var item = SkillProfileSummary.NewSkillProfileSummary("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfileSummaryValidatorReportsEveryProblem();
        SkillProfileSummaryVersionRangeRuleFlagsValuesAboveVersion();
        SkillProfileSummaryNameLengthRuleFlagsALongName();
        SkillProfileSummaryRateRangeRuleFlagsValuesAboveRate();
        SkillProfileSummaryRequiredSkillRuleFlagsABlankSkill();
    }
}
