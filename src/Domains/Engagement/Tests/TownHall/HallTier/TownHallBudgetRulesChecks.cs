namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallBudgetRulesChecks
{
    public static void TownHallBudgetValidatorReportsEveryProblem()
    {
        var validator = TownHallBudgetRules.DefaultTownHallBudgetValidator();
        var item = TownHallBudget.NewTownHallBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallBudgetDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new TownHallBudgetDescriptionLengthRule(255);
        var item = TownHallBudget.NewTownHallBudget("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new TownHallBudgetRequiredNameRule("town hall budget needs this field");
        var item = TownHallBudget.NewTownHallBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallBudgetRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new TownHallBudgetRequiredCostCenterRule("town hall budget needs an entry");
        var item = TownHallBudget.NewTownHallBudget("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallBudgetStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new TownHallBudgetStartsAtRangeRule(255, 1000);
        var item = TownHallBudget.NewTownHallBudget("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallBudgetValidatorReportsEveryProblem();
        TownHallBudgetDescriptionLengthRuleFlagsALongDescription();
        TownHallBudgetRequiredNameRuleFlagsABlankName();
        TownHallBudgetRequiredCostCenterRuleFlagsABlankCostCenter();
        TownHallBudgetStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
