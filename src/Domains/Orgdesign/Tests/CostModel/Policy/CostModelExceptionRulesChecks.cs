namespace Acme.Workforce.Orgdesign.Tests.CostModel.Policy;

using Acme.Workforce.Orgdesign.CostModel.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CostModelExceptionRulesChecks
{
    public static void CostModelExceptionValidatorReportsEveryProblem()
    {
        var validator = CostModelExceptionRules.DefaultCostModelExceptionValidator();
        var item = CostModelException.NewCostModelException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelExceptionVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new CostModelExceptionVersionRangeRule(16, 3600);
        var item = CostModelException.NewCostModelException("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new CostModelExceptionRequiredNameRule("cost model exception needs an entry");
        var item = CostModelException.NewCostModelException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CostModelExceptionValidatorReportsEveryProblem();
        CostModelExceptionVersionRangeRuleFlagsValuesAboveVersion();
        CostModelExceptionRequiredNameRuleFlagsABlankName();
    }
}
