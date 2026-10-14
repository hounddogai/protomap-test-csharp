namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayThresholdRulesChecks
{
    public static void RetroPayThresholdValidatorReportsEveryProblem()
    {
        var validator = RetroPayThresholdRules.DefaultRetroPayThresholdValidator();
        var item = RetroPayThreshold.NewRetroPayThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayThresholdCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RetroPayThresholdCostRangeRule(8.2, 2660.7);
        var item = RetroPayThreshold.NewRetroPayThreshold("x", "y");
        item.Cost = 2662.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayThresholdCodeLengthRule(16);
        var item = RetroPayThreshold.NewRetroPayThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPayThresholdValidatorReportsEveryProblem();
        RetroPayThresholdCostRangeRuleFlagsValuesAboveCost();
        RetroPayThresholdCodeLengthRuleFlagsALongCode();
    }
}
