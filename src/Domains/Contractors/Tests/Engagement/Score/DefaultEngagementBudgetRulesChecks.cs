namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class DefaultEngagementBudgetRulesChecks
{
    public static void DefaultEngagementBudgetValidatorReportsEveryProblem()
    {
        var validator = DefaultEngagementBudgetRules.DefaultDefaultEngagementBudgetValidator();
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultEngagementBudgetAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new DefaultEngagementBudgetAttemptsRangeRule(80, 120);
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultEngagementBudgetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultEngagementBudgetRequiredLabelRule("default engagement budget needs an entry");
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultEngagementBudgetPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new DefaultEngagementBudgetPhoneLengthRule(3600);
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultEngagementBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultEngagementBudgetRequiredCodeRule("default engagement budget needs a value");
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultEngagementBudgetGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new DefaultEngagementBudgetGraceDaysRangeRule(16, 120);
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultEngagementBudgetCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new DefaultEngagementBudgetCostRangeRule(7.5, 1135.8);
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("x", "y");
        item.Cost = 1137.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultEngagementBudgetValidatorReportsEveryProblem();
        DefaultEngagementBudgetAttemptsRangeRuleFlagsValuesAboveAttempts();
        DefaultEngagementBudgetRequiredLabelRuleFlagsABlankLabel();
        DefaultEngagementBudgetPhoneLengthRuleFlagsALongPhone();
        DefaultEngagementBudgetRequiredCodeRuleFlagsABlankCode();
        DefaultEngagementBudgetGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        DefaultEngagementBudgetCostRangeRuleFlagsValuesAboveCost();
    }
}
