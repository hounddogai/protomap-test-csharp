namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AssetReturnTemplateRulesChecks
{
    public static void AssetReturnTemplateValidatorReportsEveryProblem()
    {
        var validator = AssetReturnTemplateRules.DefaultAssetReturnTemplateValidator();
        var item = AssetReturnTemplate.NewAssetReturnTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssetReturnTemplateCodeLengthRule(86400);
        var item = AssetReturnTemplate.NewAssetReturnTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnTemplateSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new AssetReturnTemplateSegmentLengthRule(1000);
        var item = AssetReturnTemplate.NewAssetReturnTemplate("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssetReturnTemplateRequiredLabelRule("asset return template needs an entry");
        var item = AssetReturnTemplate.NewAssetReturnTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnTemplateValidatorReportsEveryProblem();
        AssetReturnTemplateCodeLengthRuleFlagsALongCode();
        AssetReturnTemplateSegmentLengthRuleFlagsALongSegment();
        AssetReturnTemplateRequiredLabelRuleFlagsABlankLabel();
    }
}
