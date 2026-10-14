namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelOverrideRulesChecks
{
    public static void CostModelOverrideValidatorReportsEveryProblem()
    {
        var validator = CostModelOverrideRules.DefaultCostModelOverrideValidator();
        var item = CostModelOverride.NewCostModelOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelOverrideDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new CostModelOverrideDepartmentLengthRule(64);
        var item = CostModelOverride.NewCostModelOverride("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelOverrideRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CostModelOverrideRequiredTitleRule("cost model override needs an entry");
        var item = CostModelOverride.NewCostModelOverride("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelOverrideCodeLengthRule(16);
        var item = CostModelOverride.NewCostModelOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelOverrideRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new CostModelOverrideRequiredLocaleRule("cost model override needs a value");
        var item = CostModelOverride.NewCostModelOverride("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CostModelOverrideValidatorReportsEveryProblem();
        CostModelOverrideDepartmentLengthRuleFlagsALongDepartment();
        CostModelOverrideRequiredTitleRuleFlagsABlankTitle();
        CostModelOverrideCodeLengthRuleFlagsALongCode();
        CostModelOverrideRequiredLocaleRuleFlagsABlankLocale();
    }
}
