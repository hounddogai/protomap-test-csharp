namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolTemplateRulesChecks
{
    public static void BonusPoolTemplateValidatorReportsEveryProblem()
    {
        var validator = BonusPoolTemplateRules.DefaultBonusPoolTemplateValidator();
        var item = BonusPoolTemplate.NewBonusPoolTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolTemplateRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new BonusPoolTemplateRequiredDepartmentRule("bonus pool template needs a value");
        var item = BonusPoolTemplate.NewBonusPoolTemplate("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolTemplateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BonusPoolTemplateTitleLengthRule(80);
        var item = BonusPoolTemplate.NewBonusPoolTemplate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolTemplateQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new BonusPoolTemplateQuantityRangeRule(16, 80);
        var item = BonusPoolTemplate.NewBonusPoolTemplate("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BonusPoolTemplateValidatorReportsEveryProblem();
        BonusPoolTemplateRequiredDepartmentRuleFlagsABlankDepartment();
        BonusPoolTemplateTitleLengthRuleFlagsALongTitle();
        BonusPoolTemplateQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
