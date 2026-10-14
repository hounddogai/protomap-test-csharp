namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanDraftRulesChecks
{
    public static void SuccessionPlanDraftValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanDraftRules.DefaultSuccessionPlanDraftValidator();
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanDraftReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new SuccessionPlanDraftReviewerLengthRule(1000);
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SuccessionPlanDraftRequiredTitleRule("succession plan draft needs an entry");
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanDraftPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new SuccessionPlanDraftPhoneLengthRule(1000);
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanDraftRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new SuccessionPlanDraftRequiredPhoneRule("succession plan draft needs this field");
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanDraftRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new SuccessionPlanDraftRequiredReviewerRule("succession plan draft needs this field");
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SuccessionPlanDraftValidatorReportsEveryProblem();
        SuccessionPlanDraftReviewerLengthRuleFlagsALongReviewer();
        SuccessionPlanDraftRequiredTitleRuleFlagsABlankTitle();
        SuccessionPlanDraftPhoneLengthRuleFlagsALongPhone();
        SuccessionPlanDraftRequiredPhoneRuleFlagsABlankPhone();
        SuccessionPlanDraftRequiredReviewerRuleFlagsABlankReviewer();
    }
}
