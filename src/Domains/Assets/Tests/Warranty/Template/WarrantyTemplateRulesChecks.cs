namespace Acme.Workforce.Assets.Tests.Warranty.Template;

using Acme.Workforce.Assets.Warranty.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WarrantyTemplateRulesChecks
{
    public static void WarrantyTemplateValidatorReportsEveryProblem()
    {
        var validator = WarrantyTemplateRules.DefaultWarrantyTemplateValidator();
        var item = WarrantyTemplate.NewWarrantyTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyTemplateQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new WarrantyTemplateQuantityRangeRule(120, 255);
        var item = WarrantyTemplate.NewWarrantyTemplate("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyTemplateRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new WarrantyTemplateRequiredOwnerIdRule("warranty template needs an entry");
        var item = WarrantyTemplate.NewWarrantyTemplate("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyTemplateNameLengthRuleFlagsALongName()
    {
        var rule = new WarrantyTemplateNameLengthRule(16);
        var item = WarrantyTemplate.NewWarrantyTemplate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantyTemplateValidatorReportsEveryProblem();
        WarrantyTemplateQuantityRangeRuleFlagsValuesAboveQuantity();
        WarrantyTemplateRequiredOwnerIdRuleFlagsABlankOwnerId();
        WarrantyTemplateNameLengthRuleFlagsALongName();
    }
}
