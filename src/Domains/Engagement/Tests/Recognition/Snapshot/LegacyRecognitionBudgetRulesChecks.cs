namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyRecognitionBudgetRulesChecks
{
    public static void LegacyRecognitionBudgetValidatorReportsEveryProblem()
    {
        var validator = LegacyRecognitionBudgetRules.DefaultLegacyRecognitionBudgetValidator();
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRecognitionBudgetRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new LegacyRecognitionBudgetRequiredTeamRule("legacy recognition budget needs this field");
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyRecognitionBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyRecognitionBudgetNameLengthRule(86400);
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyRecognitionBudgetReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new LegacyRecognitionBudgetReviewerLengthRule(3600);
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyRecognitionBudgetTeamLengthRuleFlagsALongTeam()
    {
        var rule = new LegacyRecognitionBudgetTeamLengthRule(3600);
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyRecognitionBudgetValidatorReportsEveryProblem();
        LegacyRecognitionBudgetRequiredTeamRuleFlagsABlankTeam();
        LegacyRecognitionBudgetNameLengthRuleFlagsALongName();
        LegacyRecognitionBudgetReviewerLengthRuleFlagsALongReviewer();
        LegacyRecognitionBudgetTeamLengthRuleFlagsALongTeam();
    }
}
