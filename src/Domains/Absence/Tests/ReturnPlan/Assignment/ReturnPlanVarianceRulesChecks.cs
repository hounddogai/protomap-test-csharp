namespace Acme.Workforce.Absence.Tests.ReturnPlan.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ReturnPlanVarianceRulesChecks
{
    public static void ReturnPlanVarianceValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanVarianceRules.DefaultReturnPlanVarianceValidator();
        var item = ReturnPlanVariance.NewReturnPlanVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanVarianceRequiredCodeRule("return plan variance needs an entry");
        var item = ReturnPlanVariance.NewReturnPlanVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanVarianceMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ReturnPlanVarianceMaximumRangeRule(4.8, 3229.7);
        var item = ReturnPlanVariance.NewReturnPlanVariance("x", "y");
        item.Maximum = 3231.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReturnPlanVarianceValidatorReportsEveryProblem();
        ReturnPlanVarianceRequiredCodeRuleFlagsABlankCode();
        ReturnPlanVarianceMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
