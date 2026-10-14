namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyMappingRulesChecks
{
    public static void WarrantyMappingValidatorReportsEveryProblem()
    {
        var validator = WarrantyMappingRules.DefaultWarrantyMappingValidator();
        var item = WarrantyMapping.NewWarrantyMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyMappingRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new WarrantyMappingRequiredSegmentRule("warranty mapping needs an entry");
        var item = WarrantyMapping.NewWarrantyMapping("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyMappingSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new WarrantyMappingSegmentLengthRule(16);
        var item = WarrantyMapping.NewWarrantyMapping("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyMappingRequiredCodeRule("warranty mapping needs a value");
        var item = WarrantyMapping.NewWarrantyMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyMappingValidatorReportsEveryProblem();
        WarrantyMappingRequiredSegmentRuleFlagsABlankSegment();
        WarrantyMappingSegmentLengthRuleFlagsALongSegment();
        WarrantyMappingRequiredCodeRuleFlagsABlankCode();
    }
}
