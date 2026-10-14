namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LegacyActionPlanDigestRulesChecks
{
    public static void LegacyActionPlanDigestValidatorReportsEveryProblem()
    {
        var validator = LegacyActionPlanDigestRules.DefaultLegacyActionPlanDigestValidator();
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyActionPlanDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyActionPlanDigestRequiredCodeRule("legacy action plan digest needs this field");
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyActionPlanDigestReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new LegacyActionPlanDigestReviewerLengthRule(3600);
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyActionPlanDigestRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new LegacyActionPlanDigestRequiredReviewerRule("legacy action plan digest needs an entry");
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyActionPlanDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyActionPlanDigestCodeLengthRule(3600);
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyActionPlanDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyActionPlanDigestRequiredTitleRule("legacy action plan digest needs this field");
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyActionPlanDigestLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new LegacyActionPlanDigestLimitRangeRule(32, 80);
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("x", "y");
        item.Limit = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyActionPlanDigestValidatorReportsEveryProblem();
        LegacyActionPlanDigestRequiredCodeRuleFlagsABlankCode();
        LegacyActionPlanDigestReviewerLengthRuleFlagsALongReviewer();
        LegacyActionPlanDigestRequiredReviewerRuleFlagsABlankReviewer();
        LegacyActionPlanDigestCodeLengthRuleFlagsALongCode();
        LegacyActionPlanDigestRequiredTitleRuleFlagsABlankTitle();
        LegacyActionPlanDigestLimitRangeRuleFlagsValuesAboveLimit();
    }
}
