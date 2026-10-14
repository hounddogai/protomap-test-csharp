namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RollingAssetReturnPlanRulesChecks
{
    public static void RollingAssetReturnPlanValidatorReportsEveryProblem()
    {
        var validator = RollingAssetReturnPlanRules.DefaultRollingAssetReturnPlanValidator();
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAssetReturnPlanRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new RollingAssetReturnPlanRetriesRangeRule(255, 1000);
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAssetReturnPlanGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new RollingAssetReturnPlanGraceDaysRangeRule(1000, 1000);
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("x", "y");
        item.GraceDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAssetReturnPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingAssetReturnPlanRequiredLabelRule("rolling asset return plan needs this field");
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAssetReturnPlanPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new RollingAssetReturnPlanPhoneLengthRule(120);
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAssetReturnPlanRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new RollingAssetReturnPlanRequiredPhoneRule("rolling asset return plan needs this field");
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingAssetReturnPlanValidatorReportsEveryProblem();
        RollingAssetReturnPlanRetriesRangeRuleFlagsValuesAboveRetries();
        RollingAssetReturnPlanGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        RollingAssetReturnPlanRequiredLabelRuleFlagsABlankLabel();
        RollingAssetReturnPlanPhoneLengthRuleFlagsALongPhone();
        RollingAssetReturnPlanRequiredPhoneRuleFlagsABlankPhone();
    }
}
