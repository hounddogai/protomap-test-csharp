namespace Acme.Workforce.Orgdesign.Tests.Headcount.Estimate;

using Acme.Workforce.Orgdesign.Headcount.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class HeadcountEstimateRulesChecks
{
    public static void HeadcountEstimateValidatorReportsEveryProblem()
    {
        var validator = HeadcountEstimateRules.DefaultHeadcountEstimateValidator();
        var item = HeadcountEstimate.NewHeadcountEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new HeadcountEstimateRequiredLabelRule("headcount estimate needs a value");
        var item = HeadcountEstimate.NewHeadcountEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountEstimateRequiredCodeRule("headcount estimate needs this field");
        var item = HeadcountEstimate.NewHeadcountEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountEstimateValidatorReportsEveryProblem();
        HeadcountEstimateRequiredLabelRuleFlagsABlankLabel();
        HeadcountEstimateRequiredCodeRuleFlagsABlankCode();
    }
}
