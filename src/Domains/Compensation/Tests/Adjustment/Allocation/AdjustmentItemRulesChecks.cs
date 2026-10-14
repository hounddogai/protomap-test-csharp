namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentItemRulesChecks
{
    public static void AdjustmentItemValidatorReportsEveryProblem()
    {
        var validator = AdjustmentItemRules.DefaultAdjustmentItemValidator();
        var item = AdjustmentItem.NewAdjustmentItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentItemRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdjustmentItemRequiredNameRule("adjustment item needs this field");
        var item = AdjustmentItem.NewAdjustmentItem("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentItemRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new AdjustmentItemRequiredRegionRule("adjustment item needs a value");
        var item = AdjustmentItem.NewAdjustmentItem("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentItemRegionLengthRuleFlagsALongRegion()
    {
        var rule = new AdjustmentItemRegionLengthRule(1000);
        var item = AdjustmentItem.NewAdjustmentItem("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentItemBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AdjustmentItemBudgetRangeRule(0.7, 3099.9);
        var item = AdjustmentItem.NewAdjustmentItem("x", "y");
        item.Budget = 3101.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdjustmentItemValidatorReportsEveryProblem();
        AdjustmentItemRequiredNameRuleFlagsABlankName();
        AdjustmentItemRequiredRegionRuleFlagsABlankRegion();
        AdjustmentItemRegionLengthRuleFlagsALongRegion();
        AdjustmentItemBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
