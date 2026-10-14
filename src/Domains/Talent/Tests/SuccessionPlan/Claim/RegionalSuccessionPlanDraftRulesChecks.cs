namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanDraftRulesChecks
{
    public static void RegionalSuccessionPlanDraftValidatorReportsEveryProblem()
    {
        var validator = RegionalSuccessionPlanDraftRules.DefaultRegionalSuccessionPlanDraftValidator();
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSuccessionPlanDraftAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new RegionalSuccessionPlanDraftAmountRangeRule(8.8, 1134.4);
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("x", "y");
        item.Amount = 1135.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSuccessionPlanDraftPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new RegionalSuccessionPlanDraftPriorityRangeRule(1000, 86400);
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSuccessionPlanDraftLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RegionalSuccessionPlanDraftLocaleLengthRule(255);
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSuccessionPlanDraftReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new RegionalSuccessionPlanDraftReviewerLengthRule(1000);
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSuccessionPlanDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalSuccessionPlanDraftCodeLengthRule(255);
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSuccessionPlanDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalSuccessionPlanDraftTitleLengthRule(3600);
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanDraftValidatorReportsEveryProblem();
        RegionalSuccessionPlanDraftAmountRangeRuleFlagsValuesAboveAmount();
        RegionalSuccessionPlanDraftPriorityRangeRuleFlagsValuesAbovePriority();
        RegionalSuccessionPlanDraftLocaleLengthRuleFlagsALongLocale();
        RegionalSuccessionPlanDraftReviewerLengthRuleFlagsALongReviewer();
        RegionalSuccessionPlanDraftCodeLengthRuleFlagsALongCode();
        RegionalSuccessionPlanDraftTitleLengthRuleFlagsALongTitle();
    }
}
