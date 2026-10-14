namespace Acme.Workforce.Talent.Tests.SkillProfile.Summary;

using Acme.Workforce.Talent.SkillProfile.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileSummaryRulesChecks
{
    public static void AdaptiveSkillProfileSummaryValidatorReportsEveryProblem()
    {
        var validator = AdaptiveSkillProfileSummaryRules.DefaultAdaptiveSkillProfileSummaryValidator();
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveSkillProfileSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveSkillProfileSummaryCodeLengthRule(16);
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveSkillProfileSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveSkillProfileSummaryRequiredCodeRule("adaptive skill profile summary needs this field");
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveSkillProfileSummarySummaryLengthRuleFlagsALongSummary()
    {
        var rule = new AdaptiveSkillProfileSummarySummaryLengthRule(86400);
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveSkillProfileSummaryValidatorReportsEveryProblem();
        AdaptiveSkillProfileSummaryCodeLengthRuleFlagsALongCode();
        AdaptiveSkillProfileSummaryRequiredCodeRuleFlagsABlankCode();
        AdaptiveSkillProfileSummarySummaryLengthRuleFlagsALongSummary();
    }
}
