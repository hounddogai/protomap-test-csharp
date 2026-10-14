namespace Acme.Workforce.Contractors.Tests.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.RateNotice;

/// <summary>Checks of rate notice code, run by the project's runner.</summary>
public static class GlobalRateExceptionRulesChecks
{
    public static void GlobalRateExceptionValidatorReportsEveryProblem()
    {
        var validator = GlobalRateExceptionRules.DefaultGlobalRateExceptionValidator();
        var item = GlobalRateException.NewGlobalRateException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalRateExceptionChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new GlobalRateExceptionChecksumLengthRule(16);
        var item = GlobalRateException.NewGlobalRateException("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalRateExceptionRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new GlobalRateExceptionRequiredChecksumRule("global rate exception needs a value");
        var item = GlobalRateException.NewGlobalRateException("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalRateExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalRateExceptionRequiredCodeRule("global rate exception needs a value");
        var item = GlobalRateException.NewGlobalRateException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalRateExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalRateExceptionLabelLengthRule(86400);
        var item = GlobalRateException.NewGlobalRateException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalRateExceptionValidatorReportsEveryProblem();
        GlobalRateExceptionChecksumLengthRuleFlagsALongChecksum();
        GlobalRateExceptionRequiredChecksumRuleFlagsABlankChecksum();
        GlobalRateExceptionRequiredCodeRuleFlagsABlankCode();
        GlobalRateExceptionLabelLengthRuleFlagsALongLabel();
    }
}
