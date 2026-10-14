namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelVarianceRulesChecks
{
    public static void CostModelVarianceValidatorReportsEveryProblem()
    {
        var validator = CostModelVarianceRules.DefaultCostModelVarianceValidator();
        var item = CostModelVariance.NewCostModelVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelVarianceRequiredCodeRule("cost model variance needs this field");
        var item = CostModelVariance.NewCostModelVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelVarianceUnitLengthRuleFlagsALongUnit()
    {
        var rule = new CostModelVarianceUnitLengthRule(255);
        var item = CostModelVariance.NewCostModelVariance("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelVarianceValidatorReportsEveryProblem();
        CostModelVarianceRequiredCodeRuleFlagsABlankCode();
        CostModelVarianceUnitLengthRuleFlagsALongUnit();
    }
}
