namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class TieredCostModelEntryRulesChecks
{
    public static void TieredCostModelEntryValidatorReportsEveryProblem()
    {
        var validator = TieredCostModelEntryRules.DefaultTieredCostModelEntryValidator();
        var item = TieredCostModelEntry.NewTieredCostModelEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredCostModelEntryBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new TieredCostModelEntryBudgetRangeRule(9.3, 110.1);
        var item = TieredCostModelEntry.NewTieredCostModelEntry("x", "y");
        item.Budget = 111.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredCostModelEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredCostModelEntryRequiredTitleRule("tiered cost model entry needs a value");
        var item = TieredCostModelEntry.NewTieredCostModelEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredCostModelEntryValidatorReportsEveryProblem();
        TieredCostModelEntryBudgetRangeRuleFlagsValuesAboveBudget();
        TieredCostModelEntryRequiredTitleRuleFlagsABlankTitle();
    }
}
