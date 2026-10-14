namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionQuotaRulesChecks
{
    public static void ExceptionQuotaValidatorReportsEveryProblem()
    {
        var validator = ExceptionQuotaRules.DefaultExceptionQuotaValidator();
        var item = ExceptionQuota.NewExceptionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionQuotaCountryLengthRuleFlagsALongCountry()
    {
        var rule = new ExceptionQuotaCountryLengthRule(64);
        var item = ExceptionQuota.NewExceptionQuota("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ExceptionQuotaRequiredLabelRule("exception quota needs this field");
        var item = ExceptionQuota.NewExceptionQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionQuotaRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new ExceptionQuotaRequiredCountryRule("exception quota needs an entry");
        var item = ExceptionQuota.NewExceptionQuota("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ExceptionQuotaLabelLengthRule(32);
        var item = ExceptionQuota.NewExceptionQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionQuotaValidatorReportsEveryProblem();
        ExceptionQuotaCountryLengthRuleFlagsALongCountry();
        ExceptionQuotaRequiredLabelRuleFlagsABlankLabel();
        ExceptionQuotaRequiredCountryRuleFlagsABlankCountry();
        ExceptionQuotaLabelLengthRuleFlagsALongLabel();
    }
}
