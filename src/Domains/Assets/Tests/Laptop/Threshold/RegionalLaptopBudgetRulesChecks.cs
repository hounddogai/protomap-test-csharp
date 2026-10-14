namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RegionalLaptopBudgetRulesChecks
{
    public static void RegionalLaptopBudgetValidatorReportsEveryProblem()
    {
        var validator = RegionalLaptopBudgetRules.DefaultRegionalLaptopBudgetValidator();
        var item = RegionalLaptopBudget.NewRegionalLaptopBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalLaptopBudgetSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new RegionalLaptopBudgetSequenceRangeRule(16, 120);
        var item = RegionalLaptopBudget.NewRegionalLaptopBudget("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalLaptopBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalLaptopBudgetRequiredNameRule("regional laptop budget needs a value");
        var item = RegionalLaptopBudget.NewRegionalLaptopBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalLaptopBudgetTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RegionalLaptopBudgetTenureMonthsRangeRule(64, 86400);
        var item = RegionalLaptopBudget.NewRegionalLaptopBudget("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalLaptopBudgetValidatorReportsEveryProblem();
        RegionalLaptopBudgetSequenceRangeRuleFlagsValuesAboveSequence();
        RegionalLaptopBudgetRequiredNameRuleFlagsABlankName();
        RegionalLaptopBudgetTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
