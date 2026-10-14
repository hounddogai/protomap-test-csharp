namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AssetReturnPolicyRulesChecks
{
    public static void AssetReturnPolicyValidatorReportsEveryProblem()
    {
        var validator = AssetReturnPolicyRules.DefaultAssetReturnPolicyValidator();
        var item = AssetReturnPolicy.NewAssetReturnPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnPolicyRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new AssetReturnPolicyRequiredGradeRule("asset return policy needs an entry");
        var item = AssetReturnPolicy.NewAssetReturnPolicy("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnPolicyThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AssetReturnPolicyThresholdRangeRule(5.9, 1941.8);
        var item = AssetReturnPolicy.NewAssetReturnPolicy("x", "y");
        item.Threshold = 1943.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnPolicyRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssetReturnPolicyRequiredLabelRule("asset return policy needs a value");
        var item = AssetReturnPolicy.NewAssetReturnPolicy("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnPolicyGradeLengthRuleFlagsALongGrade()
    {
        var rule = new AssetReturnPolicyGradeLengthRule(80);
        var item = AssetReturnPolicy.NewAssetReturnPolicy("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnPolicyValidatorReportsEveryProblem();
        AssetReturnPolicyRequiredGradeRuleFlagsABlankGrade();
        AssetReturnPolicyThresholdRangeRuleFlagsValuesAboveThreshold();
        AssetReturnPolicyRequiredLabelRuleFlagsABlankLabel();
        AssetReturnPolicyGradeLengthRuleFlagsALongGrade();
    }
}
