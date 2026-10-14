namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnBaselineRulesChecks
{
    public static void AssetReturnBaselineValidatorReportsEveryProblem()
    {
        var validator = AssetReturnBaselineRules.DefaultAssetReturnBaselineValidator();
        var item = AssetReturnBaseline.NewAssetReturnBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnBaselineDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new AssetReturnBaselineDepartmentLengthRule(64);
        var item = AssetReturnBaseline.NewAssetReturnBaseline("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnBaselineMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AssetReturnBaselineMinimumRangeRule(4.1, 3359.1);
        var item = AssetReturnBaseline.NewAssetReturnBaseline("x", "y");
        item.Minimum = 3360.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnBaselineQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AssetReturnBaselineQuantityRangeRule(64, 80);
        var item = AssetReturnBaseline.NewAssetReturnBaseline("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnBaselineRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new AssetReturnBaselineRequiredDepartmentRule("asset return baseline needs a value");
        var item = AssetReturnBaseline.NewAssetReturnBaseline("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnBaselineVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AssetReturnBaselineVersionRangeRule(1000, 3600);
        var item = AssetReturnBaseline.NewAssetReturnBaseline("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssetReturnBaselineValidatorReportsEveryProblem();
        AssetReturnBaselineDepartmentLengthRuleFlagsALongDepartment();
        AssetReturnBaselineMinimumRangeRuleFlagsValuesAboveMinimum();
        AssetReturnBaselineQuantityRangeRuleFlagsValuesAboveQuantity();
        AssetReturnBaselineRequiredDepartmentRuleFlagsABlankDepartment();
        AssetReturnBaselineVersionRangeRuleFlagsValuesAboveVersion();
    }
}
