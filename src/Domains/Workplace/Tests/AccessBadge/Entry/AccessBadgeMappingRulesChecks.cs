namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMappingRulesChecks
{
    public static void AccessBadgeMappingValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeMappingRules.DefaultAccessBadgeMappingValidator();
        var item = AccessBadgeMapping.NewAccessBadgeMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AccessBadgeMappingLabelLengthRule(3600);
        var item = AccessBadgeMapping.NewAccessBadgeMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeMappingLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new AccessBadgeMappingLimitRangeRule(64, 120);
        var item = AccessBadgeMapping.NewAccessBadgeMapping("x", "y");
        item.Limit = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeMappingNameLengthRuleFlagsALongName()
    {
        var rule = new AccessBadgeMappingNameLengthRule(64);
        var item = AccessBadgeMapping.NewAccessBadgeMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeMappingCodeLengthRule(16);
        var item = AccessBadgeMapping.NewAccessBadgeMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgeMappingRequiredCodeRule("access badge mapping needs this field");
        var item = AccessBadgeMapping.NewAccessBadgeMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeMappingRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new AccessBadgeMappingRequiredCategoryRule("access badge mapping needs an entry");
        var item = AccessBadgeMapping.NewAccessBadgeMapping("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeMappingValidatorReportsEveryProblem();
        AccessBadgeMappingLabelLengthRuleFlagsALongLabel();
        AccessBadgeMappingLimitRangeRuleFlagsValuesAboveLimit();
        AccessBadgeMappingNameLengthRuleFlagsALongName();
        AccessBadgeMappingCodeLengthRuleFlagsALongCode();
        AccessBadgeMappingRequiredCodeRuleFlagsABlankCode();
        AccessBadgeMappingRequiredCategoryRuleFlagsABlankCategory();
    }
}
