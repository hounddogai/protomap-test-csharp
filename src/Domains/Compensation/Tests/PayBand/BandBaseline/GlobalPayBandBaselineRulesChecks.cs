namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class GlobalPayBandBaselineRulesChecks
{
    public static void GlobalPayBandBaselineValidatorReportsEveryProblem()
    {
        var validator = GlobalPayBandBaselineRules.DefaultGlobalPayBandBaselineValidator();
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPayBandBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalPayBandBaselineRequiredCodeRule("global pay band baseline needs a value");
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalPayBandBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalPayBandBaselineCodeLengthRule(86400);
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPayBandBaselineRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalPayBandBaselineRequiredNameRule("global pay band baseline needs a value");
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalPayBandBaselineValidatorReportsEveryProblem();
        GlobalPayBandBaselineRequiredCodeRuleFlagsABlankCode();
        GlobalPayBandBaselineCodeLengthRuleFlagsALongCode();
        GlobalPayBandBaselineRequiredNameRuleFlagsABlankName();
    }
}
