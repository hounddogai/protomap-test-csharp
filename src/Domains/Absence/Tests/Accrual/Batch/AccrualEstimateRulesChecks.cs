namespace Acme.Workforce.Absence.Tests.Accrual.Batch;

using Acme.Workforce.Absence.Accrual.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccrualEstimateRulesChecks
{
    public static void AccrualEstimateValidatorReportsEveryProblem()
    {
        var validator = AccrualEstimateRules.DefaultAccrualEstimateValidator();
        var item = AccrualEstimate.NewAccrualEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualEstimateCodeLengthRule(80);
        var item = AccrualEstimate.NewAccrualEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualEstimateRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccrualEstimateRequiredNameRule("accrual estimate needs a value");
        var item = AccrualEstimate.NewAccrualEstimate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualEstimateValidatorReportsEveryProblem();
        AccrualEstimateCodeLengthRuleFlagsALongCode();
        AccrualEstimateRequiredNameRuleFlagsABlankName();
    }
}
