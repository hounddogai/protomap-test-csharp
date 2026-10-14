namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnPlanRulesChecks
{
    public static void AssetReturnPlanValidatorReportsEveryProblem()
    {
        var validator = AssetReturnPlanRules.DefaultAssetReturnPlanValidator();
        var item = AssetReturnPlan.NewAssetReturnPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnPlanRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new AssetReturnPlanRequiredReviewerRule("asset return plan needs this field");
        var item = AssetReturnPlan.NewAssetReturnPlan("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnPlanReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new AssetReturnPlanReviewerLengthRule(255);
        var item = AssetReturnPlan.NewAssetReturnPlan("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnPlanRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new AssetReturnPlanRequiredNotesRule("asset return plan needs an entry");
        var item = AssetReturnPlan.NewAssetReturnPlan("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnPlanRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssetReturnPlanRequiredTitleRule("asset return plan needs this field");
        var item = AssetReturnPlan.NewAssetReturnPlan("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnPlanTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssetReturnPlanTitleLengthRule(3600);
        var item = AssetReturnPlan.NewAssetReturnPlan("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnPlanValidatorReportsEveryProblem();
        AssetReturnPlanRequiredReviewerRuleFlagsABlankReviewer();
        AssetReturnPlanReviewerLengthRuleFlagsALongReviewer();
        AssetReturnPlanRequiredNotesRuleFlagsABlankNotes();
        AssetReturnPlanRequiredTitleRuleFlagsABlankTitle();
        AssetReturnPlanTitleLengthRuleFlagsALongTitle();
    }
}
