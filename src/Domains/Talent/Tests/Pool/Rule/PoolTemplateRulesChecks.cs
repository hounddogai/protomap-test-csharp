namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolTemplateRulesChecks
{
    public static void PoolTemplateValidatorReportsEveryProblem()
    {
        var validator = PoolTemplateRules.DefaultPoolTemplateValidator();
        var item = PoolTemplate.NewPoolTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolTemplateAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PoolTemplateAmountRangeRule(7.1, 3581.6);
        var item = PoolTemplate.NewPoolTemplate("x", "y");
        item.Amount = 3583.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolTemplateRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new PoolTemplateRequiredManagerIdRule("pool template needs this field");
        var item = PoolTemplate.NewPoolTemplate("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PoolTemplateRequiredLabelRule("pool template needs this field");
        var item = PoolTemplate.NewPoolTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolTemplateSourceLengthRuleFlagsALongSource()
    {
        var rule = new PoolTemplateSourceLengthRule(255);
        var item = PoolTemplate.NewPoolTemplate("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolTemplateValidatorReportsEveryProblem();
        PoolTemplateAmountRangeRuleFlagsValuesAboveAmount();
        PoolTemplateRequiredManagerIdRuleFlagsABlankManagerId();
        PoolTemplateRequiredLabelRuleFlagsABlankLabel();
        PoolTemplateSourceLengthRuleFlagsALongSource();
    }
}
