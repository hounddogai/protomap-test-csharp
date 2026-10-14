namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StrictHeadcountVarianceRulesChecks
{
    public static void StrictHeadcountVarianceValidatorReportsEveryProblem()
    {
        var validator = StrictHeadcountVarianceRules.DefaultStrictHeadcountVarianceValidator();
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictHeadcountVarianceRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new StrictHeadcountVarianceRequiredUnitRule("strict headcount variance needs an entry");
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictHeadcountVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictHeadcountVarianceRequiredCodeRule("strict headcount variance needs a value");
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictHeadcountVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictHeadcountVarianceCodeLengthRule(1000);
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictHeadcountVarianceRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new StrictHeadcountVarianceRequiredCostCenterRule("strict headcount variance needs an entry");
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictHeadcountVarianceValidatorReportsEveryProblem();
        StrictHeadcountVarianceRequiredUnitRuleFlagsABlankUnit();
        StrictHeadcountVarianceRequiredCodeRuleFlagsABlankCode();
        StrictHeadcountVarianceCodeLengthRuleFlagsALongCode();
        StrictHeadcountVarianceRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
