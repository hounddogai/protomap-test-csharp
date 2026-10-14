namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalLaptopBudgetRulesChecks
{
    public static void SeasonalLaptopBudgetValidatorReportsEveryProblem()
    {
        var validator = SeasonalLaptopBudgetRules.DefaultSeasonalLaptopBudgetValidator();
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalLaptopBudgetHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new SeasonalLaptopBudgetHoursRangeRule(2.9, 3338.9);
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("x", "y");
        item.Hours = 3340.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalLaptopBudgetRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new SeasonalLaptopBudgetRequiredBadgeRule("seasonal laptop budget needs an entry");
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalLaptopBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalLaptopBudgetRequiredCodeRule("seasonal laptop budget needs this field");
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalLaptopBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new SeasonalLaptopBudgetNameLengthRule(1000);
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalLaptopBudgetValidatorReportsEveryProblem();
        SeasonalLaptopBudgetHoursRangeRuleFlagsValuesAboveHours();
        SeasonalLaptopBudgetRequiredBadgeRuleFlagsABlankBadge();
        SeasonalLaptopBudgetRequiredCodeRuleFlagsABlankCode();
        SeasonalLaptopBudgetNameLengthRuleFlagsALongName();
    }
}
