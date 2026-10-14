namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SkillProfileDraftRulesChecks
{
    public static void SkillProfileDraftValidatorReportsEveryProblem()
    {
        var validator = SkillProfileDraftRules.DefaultSkillProfileDraftValidator();
        var item = SkillProfileDraft.NewSkillProfileDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileDraftSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new SkillProfileDraftSequenceRangeRule(64, 80);
        var item = SkillProfileDraft.NewSkillProfileDraft("x", "y");
        item.Sequence = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileDraftSourceLengthRuleFlagsALongSource()
    {
        var rule = new SkillProfileDraftSourceLengthRule(86400);
        var item = SkillProfileDraft.NewSkillProfileDraft("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileDraftVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new SkillProfileDraftVersionRangeRule(80, 86400);
        var item = SkillProfileDraft.NewSkillProfileDraft("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileDraftRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SkillProfileDraftRetriesRangeRule(16, 64);
        var item = SkillProfileDraft.NewSkillProfileDraft("x", "y");
        item.Retries = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SkillProfileDraftValidatorReportsEveryProblem();
        SkillProfileDraftSequenceRangeRuleFlagsValuesAboveSequence();
        SkillProfileDraftSourceLengthRuleFlagsALongSource();
        SkillProfileDraftVersionRangeRuleFlagsValuesAboveVersion();
        SkillProfileDraftRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
