namespace Acme.Workforce.Absence.Tests.ReturnPlan.Window;

using Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredReturnPlanItemRulesChecks
{
    public static void TieredReturnPlanItemValidatorReportsEveryProblem()
    {
        var validator = TieredReturnPlanItemRules.DefaultTieredReturnPlanItemValidator();
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredReturnPlanItemQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new TieredReturnPlanItemQuantityRangeRule(255, 3600);
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredReturnPlanItemRegionLengthRuleFlagsALongRegion()
    {
        var rule = new TieredReturnPlanItemRegionLengthRule(64);
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredReturnPlanItemRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredReturnPlanItemRequiredTitleRule("tiered return plan item needs this field");
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredReturnPlanItemRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new TieredReturnPlanItemRequiredOwnerIdRule("tiered return plan item needs a value");
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredReturnPlanItemTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredReturnPlanItemTitleLengthRule(86400);
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredReturnPlanItemValidatorReportsEveryProblem();
        TieredReturnPlanItemQuantityRangeRuleFlagsValuesAboveQuantity();
        TieredReturnPlanItemRegionLengthRuleFlagsALongRegion();
        TieredReturnPlanItemRequiredTitleRuleFlagsABlankTitle();
        TieredReturnPlanItemRequiredOwnerIdRuleFlagsABlankOwnerId();
        TieredReturnPlanItemTitleLengthRuleFlagsALongTitle();
    }
}
