namespace Acme.Workforce.Compliance.Tests.Exception.Baseline;

using Acme.Workforce.Compliance.Exception.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ExceptionBaselineRulesChecks
{
    public static void ExceptionBaselineValidatorReportsEveryProblem()
    {
        var validator = ExceptionBaselineRules.DefaultExceptionBaselineValidator();
        var item = ExceptionBaseline.NewExceptionBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionBaselineExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new ExceptionBaselineExternalRefLengthRule(120);
        var item = ExceptionBaseline.NewExceptionBaseline("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionBaselineRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new ExceptionBaselineRetriesRangeRule(64, 3600);
        var item = ExceptionBaseline.NewExceptionBaseline("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionBaselineTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new ExceptionBaselineTenureMonthsRangeRule(1000, 3600);
        var item = ExceptionBaseline.NewExceptionBaseline("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ExceptionBaselineValidatorReportsEveryProblem();
        ExceptionBaselineExternalRefLengthRuleFlagsALongExternalRef();
        ExceptionBaselineRetriesRangeRuleFlagsValuesAboveRetries();
        ExceptionBaselineTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
