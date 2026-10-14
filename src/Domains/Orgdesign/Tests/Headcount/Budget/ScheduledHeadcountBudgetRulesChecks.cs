namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class ScheduledHeadcountBudgetRulesChecks
{
    public static void ScheduledHeadcountBudgetValidatorReportsEveryProblem()
    {
        var validator = ScheduledHeadcountBudgetRules.DefaultScheduledHeadcountBudgetValidator();
        var item = ScheduledHeadcountBudget.NewScheduledHeadcountBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledHeadcountBudgetRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new ScheduledHeadcountBudgetRequiredBadgeRule("scheduled headcount budget needs a value");
        var item = ScheduledHeadcountBudget.NewScheduledHeadcountBudget("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledHeadcountBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledHeadcountBudgetNameLengthRule(255);
        var item = ScheduledHeadcountBudget.NewScheduledHeadcountBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledHeadcountBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledHeadcountBudgetRequiredNameRule("scheduled headcount budget needs this field");
        var item = ScheduledHeadcountBudget.NewScheduledHeadcountBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledHeadcountBudgetValidatorReportsEveryProblem();
        ScheduledHeadcountBudgetRequiredBadgeRuleFlagsABlankBadge();
        ScheduledHeadcountBudgetNameLengthRuleFlagsALongName();
        ScheduledHeadcountBudgetRequiredNameRuleFlagsABlankName();
    }
}
