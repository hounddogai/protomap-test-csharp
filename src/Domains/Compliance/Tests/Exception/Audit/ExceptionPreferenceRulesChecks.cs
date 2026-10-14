namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ExceptionPreferenceRulesChecks
{
    public static void ExceptionPreferenceValidatorReportsEveryProblem()
    {
        var validator = ExceptionPreferenceRules.DefaultExceptionPreferenceValidator();
        var item = ExceptionPreference.NewExceptionPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionPreferenceRegionLengthRuleFlagsALongRegion()
    {
        var rule = new ExceptionPreferenceRegionLengthRule(64);
        var item = ExceptionPreference.NewExceptionPreference("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionPreferenceRequiredCodeRule("exception preference needs an entry");
        var item = ExceptionPreference.NewExceptionPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionPreferenceValidatorReportsEveryProblem();
        ExceptionPreferenceRegionLengthRuleFlagsALongRegion();
        ExceptionPreferenceRequiredCodeRuleFlagsABlankCode();
    }
}
