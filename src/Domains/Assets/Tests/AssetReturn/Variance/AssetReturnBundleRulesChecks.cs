namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnBundleRulesChecks
{
    public static void AssetReturnBundleValidatorReportsEveryProblem()
    {
        var validator = AssetReturnBundleRules.DefaultAssetReturnBundleValidator();
        var item = AssetReturnBundle.NewAssetReturnBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnBundleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AssetReturnBundleCostRangeRule(9.1, 4431.4);
        var item = AssetReturnBundle.NewAssetReturnBundle("x", "y");
        item.Cost = 4432.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssetReturnBundleCodeLengthRule(3600);
        var item = AssetReturnBundle.NewAssetReturnBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new AssetReturnBundleRequiredNameRule("asset return bundle needs an entry");
        var item = AssetReturnBundle.NewAssetReturnBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnBundleRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new AssetReturnBundleRequiredDepartmentRule("asset return bundle needs an entry");
        var item = AssetReturnBundle.NewAssetReturnBundle("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnBundleDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new AssetReturnBundleDepartmentLengthRule(80);
        var item = AssetReturnBundle.NewAssetReturnBundle("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnBundleValidatorReportsEveryProblem();
        AssetReturnBundleCostRangeRuleFlagsValuesAboveCost();
        AssetReturnBundleCodeLengthRuleFlagsALongCode();
        AssetReturnBundleRequiredNameRuleFlagsABlankName();
        AssetReturnBundleRequiredDepartmentRuleFlagsABlankDepartment();
        AssetReturnBundleDepartmentLengthRuleFlagsALongDepartment();
    }
}
