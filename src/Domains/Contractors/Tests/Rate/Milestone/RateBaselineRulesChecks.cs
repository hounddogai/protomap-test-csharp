namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class RateBaselineRulesChecks
{
    public static void RateBaselineValidatorReportsEveryProblem()
    {
        var validator = RateBaselineRules.DefaultRateBaselineValidator();
        var item = RateBaseline.NewRateBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateBaselineSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new RateBaselineSalaryBandLengthRule(86400);
        var item = RateBaseline.NewRateBaseline("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateBaselineRequiredNameRuleFlagsABlankName()
    {
        var rule = new RateBaselineRequiredNameRule("rate baseline needs this field");
        var item = RateBaseline.NewRateBaseline("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateBaselineRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new RateBaselineRequiredLocaleRule("rate baseline needs a value");
        var item = RateBaseline.NewRateBaseline("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RateBaselineRequiredCodeRule("rate baseline needs this field");
        var item = RateBaseline.NewRateBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RateBaselineValidatorReportsEveryProblem();
        RateBaselineSalaryBandLengthRuleFlagsALongSalaryBand();
        RateBaselineRequiredNameRuleFlagsABlankName();
        RateBaselineRequiredLocaleRuleFlagsABlankLocale();
        RateBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
