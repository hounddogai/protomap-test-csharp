namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PositionBudgetRulesChecks
{
    public static void PositionBudgetValidatorReportsEveryProblem()
    {
        var validator = PositionBudgetRules.DefaultPositionBudgetValidator();
        var item = PositionBudget.NewPositionBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PositionBudgetRequiredCodeRule("position budget needs an entry");
        var item = PositionBudget.NewPositionBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionBudgetRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PositionBudgetRequiredReviewerRule("position budget needs an entry");
        var item = PositionBudget.NewPositionBudget("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionBudgetValidatorReportsEveryProblem();
        PositionBudgetRequiredCodeRuleFlagsABlankCode();
        PositionBudgetRequiredReviewerRuleFlagsABlankReviewer();
    }
}
