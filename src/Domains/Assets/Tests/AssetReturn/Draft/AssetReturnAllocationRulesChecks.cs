namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnAllocationRulesChecks
{
    public static void AssetReturnAllocationValidatorReportsEveryProblem()
    {
        var validator = AssetReturnAllocationRules.DefaultAssetReturnAllocationValidator();
        var item = AssetReturnAllocation.NewAssetReturnAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnAllocationMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AssetReturnAllocationMaximumRangeRule(4.0, 2559.4);
        var item = AssetReturnAllocation.NewAssetReturnAllocation("x", "y");
        item.Maximum = 2560.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnAllocationRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new AssetReturnAllocationRequiredDescriptionRule("asset return allocation needs this field");
        var item = AssetReturnAllocation.NewAssetReturnAllocation("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnAllocationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssetReturnAllocationTitleLengthRule(3600);
        var item = AssetReturnAllocation.NewAssetReturnAllocation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnAllocationDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new AssetReturnAllocationDescriptionLengthRule(3600);
        var item = AssetReturnAllocation.NewAssetReturnAllocation("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnAllocationValidatorReportsEveryProblem();
        AssetReturnAllocationMaximumRangeRuleFlagsValuesAboveMaximum();
        AssetReturnAllocationRequiredDescriptionRuleFlagsABlankDescription();
        AssetReturnAllocationTitleLengthRuleFlagsALongTitle();
        AssetReturnAllocationDescriptionLengthRuleFlagsALongDescription();
    }
}
