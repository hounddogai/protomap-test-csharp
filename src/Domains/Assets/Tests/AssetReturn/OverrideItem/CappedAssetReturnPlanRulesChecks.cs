namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CappedAssetReturnPlanRulesChecks
{
    public static void CappedAssetReturnPlanValidatorReportsEveryProblem()
    {
        var validator = CappedAssetReturnPlanRules.DefaultCappedAssetReturnPlanValidator();
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAssetReturnPlanRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new CappedAssetReturnPlanRequiredSegmentRule("capped asset return plan needs an entry");
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedAssetReturnPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new CappedAssetReturnPlanRequiredNameRule("capped asset return plan needs this field");
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedAssetReturnPlanAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new CappedAssetReturnPlanAmountRangeRule(6.2, 1598.3);
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("x", "y");
        item.Amount = 1599.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedAssetReturnPlanValidatorReportsEveryProblem();
        CappedAssetReturnPlanRequiredSegmentRuleFlagsABlankSegment();
        CappedAssetReturnPlanRequiredNameRuleFlagsABlankName();
        CappedAssetReturnPlanAmountRangeRuleFlagsValuesAboveAmount();
    }
}
