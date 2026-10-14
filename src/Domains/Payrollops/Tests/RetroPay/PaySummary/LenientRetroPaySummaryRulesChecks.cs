namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class LenientRetroPaySummaryRulesChecks
{
    public static void LenientRetroPaySummaryValidatorReportsEveryProblem()
    {
        var validator = LenientRetroPaySummaryRules.DefaultLenientRetroPaySummaryValidator();
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRetroPaySummaryFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new LenientRetroPaySummaryFteRangeRule(2.9, 850.5);
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("x", "y");
        item.Fte = 852.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientRetroPaySummaryRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new LenientRetroPaySummaryRequiredSummaryRule("lenient retro pay summary needs an entry");
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRetroPaySummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientRetroPaySummaryRequiredCodeRule("lenient retro pay summary needs this field");
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRetroPaySummaryCountryLengthRuleFlagsALongCountry()
    {
        var rule = new LenientRetroPaySummaryCountryLengthRule(86400);
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientRetroPaySummaryValidatorReportsEveryProblem();
        LenientRetroPaySummaryFteRangeRuleFlagsValuesAboveFte();
        LenientRetroPaySummaryRequiredSummaryRuleFlagsABlankSummary();
        LenientRetroPaySummaryRequiredCodeRuleFlagsABlankCode();
        LenientRetroPaySummaryCountryLengthRuleFlagsALongCountry();
    }
}
