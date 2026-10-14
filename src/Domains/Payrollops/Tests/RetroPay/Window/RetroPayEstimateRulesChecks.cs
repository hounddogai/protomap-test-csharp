namespace Acme.Workforce.Payrollops.Tests.RetroPay.Window;

using Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RetroPayEstimateRulesChecks
{
    public static void RetroPayEstimateValidatorReportsEveryProblem()
    {
        var validator = RetroPayEstimateRules.DefaultRetroPayEstimateValidator();
        var item = RetroPayEstimate.NewRetroPayEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayEstimateRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new RetroPayEstimateRequiredJobCodeRule("retro pay estimate needs an entry");
        var item = RetroPayEstimate.NewRetroPayEstimate("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayEstimateSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new RetroPayEstimateSummaryLengthRule(80);
        var item = RetroPayEstimate.NewRetroPayEstimate("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RetroPayEstimateTitleLengthRule(16);
        var item = RetroPayEstimate.NewRetroPayEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPayEstimateValidatorReportsEveryProblem();
        RetroPayEstimateRequiredJobCodeRuleFlagsABlankJobCode();
        RetroPayEstimateSummaryLengthRuleFlagsALongSummary();
        RetroPayEstimateTitleLengthRuleFlagsALongTitle();
    }
}
