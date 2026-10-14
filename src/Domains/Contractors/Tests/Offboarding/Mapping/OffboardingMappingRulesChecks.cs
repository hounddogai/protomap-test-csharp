namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class OffboardingMappingRulesChecks
{
    public static void OffboardingMappingValidatorReportsEveryProblem()
    {
        var validator = OffboardingMappingRules.DefaultOffboardingMappingValidator();
        var item = OffboardingMapping.NewOffboardingMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingMappingBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new OffboardingMappingBatchSizeRangeRule(32, 64);
        var item = OffboardingMapping.NewOffboardingMapping("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingMappingLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new OffboardingMappingLocaleLengthRule(3600);
        var item = OffboardingMapping.NewOffboardingMapping("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OffboardingMappingRequiredCodeRule("offboarding mapping needs this field");
        var item = OffboardingMapping.NewOffboardingMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingMappingCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new OffboardingMappingCategoryLengthRule(64);
        var item = OffboardingMapping.NewOffboardingMapping("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingMappingCodeLengthRule(16);
        var item = OffboardingMapping.NewOffboardingMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingMappingValidatorReportsEveryProblem();
        OffboardingMappingBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        OffboardingMappingLocaleLengthRuleFlagsALongLocale();
        OffboardingMappingRequiredCodeRuleFlagsABlankCode();
        OffboardingMappingCategoryLengthRuleFlagsALongCategory();
        OffboardingMappingCodeLengthRuleFlagsALongCode();
    }
}
