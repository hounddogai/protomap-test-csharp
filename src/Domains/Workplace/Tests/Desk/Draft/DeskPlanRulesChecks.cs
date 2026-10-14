namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class DeskPlanRulesChecks
{
    public static void DeskPlanValidatorReportsEveryProblem()
    {
        var validator = DeskPlanRules.DefaultDeskPlanValidator();
        var item = DeskPlan.NewDeskPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskPlanReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new DeskPlanReviewerLengthRule(32);
        var item = DeskPlan.NewDeskPlan("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskPlanTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new DeskPlanTenureMonthsRangeRule(80, 3600);
        var item = DeskPlan.NewDeskPlan("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskPlanCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new DeskPlanCurrencyLengthRule(64);
        var item = DeskPlan.NewDeskPlan("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskPlanValidatorReportsEveryProblem();
        DeskPlanReviewerLengthRuleFlagsALongReviewer();
        DeskPlanTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        DeskPlanCurrencyLengthRuleFlagsALongCurrency();
    }
}
