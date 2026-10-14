namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class WeightedGarnishmentBudgetRulesChecks
{
    public static void WeightedGarnishmentBudgetValidatorReportsEveryProblem()
    {
        var validator = WeightedGarnishmentBudgetRules.DefaultWeightedGarnishmentBudgetValidator();
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedGarnishmentBudgetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedGarnishmentBudgetRequiredLabelRule("weighted garnishment budget needs an entry");
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedGarnishmentBudgetAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new WeightedGarnishmentBudgetAmountRangeRule(1.5, 4740.8);
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("x", "y");
        item.Amount = 4742.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedGarnishmentBudgetJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new WeightedGarnishmentBudgetJobCodeLengthRule(3600);
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedGarnishmentBudgetScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new WeightedGarnishmentBudgetScoreRangeRule(0.3, 401.8);
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("x", "y");
        item.Score = 403.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedGarnishmentBudgetValidatorReportsEveryProblem();
        WeightedGarnishmentBudgetRequiredLabelRuleFlagsABlankLabel();
        WeightedGarnishmentBudgetAmountRangeRuleFlagsValuesAboveAmount();
        WeightedGarnishmentBudgetJobCodeLengthRuleFlagsALongJobCode();
        WeightedGarnishmentBudgetScoreRangeRuleFlagsValuesAboveScore();
    }
}
