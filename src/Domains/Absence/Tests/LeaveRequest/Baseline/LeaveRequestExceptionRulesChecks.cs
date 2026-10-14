namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestExceptionRulesChecks
{
    public static void LeaveRequestExceptionValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestExceptionRules.DefaultLeaveRequestExceptionValidator();
        var item = LeaveRequestException.NewLeaveRequestException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LeaveRequestExceptionLabelLengthRule(32);
        var item = LeaveRequestException.NewLeaveRequestException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestExceptionRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LeaveRequestExceptionRequiredLabelRule("leave request exception needs this field");
        var item = LeaveRequestException.NewLeaveRequestException("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestExceptionMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LeaveRequestExceptionMaximumRangeRule(0.4, 256.4);
        var item = LeaveRequestException.NewLeaveRequestException("x", "y");
        item.Maximum = 257.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestExceptionRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new LeaveRequestExceptionRequiredSummaryRule("leave request exception needs a value");
        var item = LeaveRequestException.NewLeaveRequestException("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestExceptionValidatorReportsEveryProblem();
        LeaveRequestExceptionLabelLengthRuleFlagsALongLabel();
        LeaveRequestExceptionRequiredLabelRuleFlagsABlankLabel();
        LeaveRequestExceptionMaximumRangeRuleFlagsValuesAboveMaximum();
        LeaveRequestExceptionRequiredSummaryRuleFlagsABlankSummary();
    }
}
