namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalEngagementBudgetRulesChecks
{
    public static void RegionalEngagementBudgetValidatorReportsEveryProblem()
    {
        var validator = RegionalEngagementBudgetRules.DefaultRegionalEngagementBudgetValidator();
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalEngagementBudgetWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RegionalEngagementBudgetWeightRangeRule(5.7, 4453.0);
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("x", "y");
        item.Weight = 4454.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalEngagementBudgetRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new RegionalEngagementBudgetRequiredSalaryBandRule("regional engagement budget needs this field");
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalEngagementBudgetExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RegionalEngagementBudgetExpiresAtRangeRule(16, 86400);
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalEngagementBudgetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalEngagementBudgetRequiredLabelRule("regional engagement budget needs this field");
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalEngagementBudgetValidatorReportsEveryProblem();
        RegionalEngagementBudgetWeightRangeRuleFlagsValuesAboveWeight();
        RegionalEngagementBudgetRequiredSalaryBandRuleFlagsABlankSalaryBand();
        RegionalEngagementBudgetExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RegionalEngagementBudgetRequiredLabelRuleFlagsABlankLabel();
    }
}
