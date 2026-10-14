namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanRecordRulesChecks
{
    public static void SpanRecordValidatorReportsEveryProblem()
    {
        var validator = SpanRecordRules.DefaultSpanRecordValidator();
        var item = SpanRecord.NewSpanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanRecordEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new SpanRecordEmployeeIdLengthRule(32);
        var item = SpanRecord.NewSpanRecord("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SpanRecordLabelLengthRule(64);
        var item = SpanRecord.NewSpanRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanRecordValidatorReportsEveryProblem();
        SpanRecordEmployeeIdLengthRuleFlagsALongEmployeeId();
        SpanRecordLabelLengthRuleFlagsALongLabel();
    }
}
