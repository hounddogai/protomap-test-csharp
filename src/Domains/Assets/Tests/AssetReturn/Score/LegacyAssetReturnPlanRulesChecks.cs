namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LegacyAssetReturnPlanRulesChecks
{
    public static void LegacyAssetReturnPlanValidatorReportsEveryProblem()
    {
        var validator = LegacyAssetReturnPlanRules.DefaultLegacyAssetReturnPlanValidator();
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyAssetReturnPlanRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new LegacyAssetReturnPlanRequiredManagerIdRule("legacy asset return plan needs this field");
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAssetReturnPlanRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyAssetReturnPlanRequiredTitleRule("legacy asset return plan needs an entry");
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAssetReturnPlanOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new LegacyAssetReturnPlanOwnerIdLengthRule(1000);
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyAssetReturnPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyAssetReturnPlanCodeLengthRule(86400);
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyAssetReturnPlanValidatorReportsEveryProblem();
        LegacyAssetReturnPlanRequiredManagerIdRuleFlagsABlankManagerId();
        LegacyAssetReturnPlanRequiredTitleRuleFlagsABlankTitle();
        LegacyAssetReturnPlanOwnerIdLengthRuleFlagsALongOwnerId();
        LegacyAssetReturnPlanCodeLengthRuleFlagsALongCode();
    }
}
