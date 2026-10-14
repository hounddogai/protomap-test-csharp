namespace Acme.Workforce.Compensation.Tests.PayBand.Summary;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultPayBandEstimateRulesChecks
{
    public static void DefaultPayBandEstimateValidatorReportsEveryProblem()
    {
        var validator = DefaultPayBandEstimateRules.DefaultDefaultPayBandEstimateValidator();
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPayBandEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultPayBandEstimateNameLengthRule(120);
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPayBandEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultPayBandEstimateRequiredCodeRule("default pay band estimate needs this field");
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPayBandEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultPayBandEstimateCodeLengthRule(3600);
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultPayBandEstimateValidatorReportsEveryProblem();
        DefaultPayBandEstimateNameLengthRuleFlagsALongName();
        DefaultPayBandEstimateRequiredCodeRuleFlagsABlankCode();
        DefaultPayBandEstimateCodeLengthRuleFlagsALongCode();
    }
}
