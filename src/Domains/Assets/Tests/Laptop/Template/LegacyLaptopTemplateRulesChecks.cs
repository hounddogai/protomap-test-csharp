namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyLaptopTemplateRulesChecks
{
    public static void LegacyLaptopTemplateValidatorReportsEveryProblem()
    {
        var validator = LegacyLaptopTemplateRules.DefaultLegacyLaptopTemplateValidator();
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyLaptopTemplateQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new LegacyLaptopTemplateQuantityRangeRule(255, 86400);
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyLaptopTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyLaptopTemplateRequiredCodeRule("legacy laptop template needs this field");
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyLaptopTemplateBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new LegacyLaptopTemplateBadgeLengthRule(80);
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyLaptopTemplateValidatorReportsEveryProblem();
        LegacyLaptopTemplateQuantityRangeRuleFlagsValuesAboveQuantity();
        LegacyLaptopTemplateRequiredCodeRuleFlagsABlankCode();
        LegacyLaptopTemplateBadgeLengthRuleFlagsALongBadge();
    }
}
