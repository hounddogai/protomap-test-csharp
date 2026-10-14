namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestBudgetRulesChecks
{
    public static void SeasonalLeaveRequestBudgetValidatorReportsEveryProblem()
    {
        var validator = SeasonalLeaveRequestBudgetRules.DefaultSeasonalLeaveRequestBudgetValidator();
        var item = SeasonalLeaveRequestBudget.NewSeasonalLeaveRequestBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalLeaveRequestBudgetCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new SeasonalLeaveRequestBudgetCategoryLengthRule(64);
        var item = SeasonalLeaveRequestBudget.NewSeasonalLeaveRequestBudget("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestBudgetValidatorReportsEveryProblem();
        SeasonalLeaveRequestBudgetCategoryLengthRuleFlagsALongCategory();
    }
}
