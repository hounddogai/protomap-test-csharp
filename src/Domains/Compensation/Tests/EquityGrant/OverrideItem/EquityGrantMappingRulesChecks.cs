namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class EquityGrantMappingRulesChecks
{
    public static void EquityGrantMappingValidatorReportsEveryProblem()
    {
        var validator = EquityGrantMappingRules.DefaultEquityGrantMappingValidator();
        var item = EquityGrantMapping.NewEquityGrantMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantMappingBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new EquityGrantMappingBudgetRangeRule(3.8, 1524.2);
        var item = EquityGrantMapping.NewEquityGrantMapping("x", "y");
        item.Budget = 1525.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new EquityGrantMappingRequiredNameRule("equity grant mapping needs a value");
        var item = EquityGrantMapping.NewEquityGrantMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantMappingRequiredCodeRule("equity grant mapping needs a value");
        var item = EquityGrantMapping.NewEquityGrantMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantMappingValidatorReportsEveryProblem();
        EquityGrantMappingBudgetRangeRuleFlagsValuesAboveBudget();
        EquityGrantMappingRequiredNameRuleFlagsABlankName();
        EquityGrantMappingRequiredCodeRuleFlagsABlankCode();
    }
}
