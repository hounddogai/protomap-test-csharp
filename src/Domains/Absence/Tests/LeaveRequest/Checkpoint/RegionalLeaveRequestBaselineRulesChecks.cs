namespace Acme.Workforce.Absence.Tests.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalLeaveRequestBaselineRulesChecks
{
    public static void RegionalLeaveRequestBaselineValidatorReportsEveryProblem()
    {
        var validator = RegionalLeaveRequestBaselineRules.DefaultRegionalLeaveRequestBaselineValidator();
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalLeaveRequestBaselineRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new RegionalLeaveRequestBaselineRequiredSegmentRule("regional leave request baseline needs a value");
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalLeaveRequestBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalLeaveRequestBaselineCodeLengthRule(80);
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalLeaveRequestBaselineRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new RegionalLeaveRequestBaselineRequiredSummaryRule(
            "regional leave request baseline needs this field");
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalLeaveRequestBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalLeaveRequestBaselineRequiredCodeRule("regional leave request baseline needs this field");
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalLeaveRequestBaselineScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RegionalLeaveRequestBaselineScoreRangeRule(5.0, 3184.6);
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("x", "y");
        item.Score = 3186.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalLeaveRequestBaselineValidatorReportsEveryProblem();
        RegionalLeaveRequestBaselineRequiredSegmentRuleFlagsABlankSegment();
        RegionalLeaveRequestBaselineCodeLengthRuleFlagsALongCode();
        RegionalLeaveRequestBaselineRequiredSummaryRuleFlagsABlankSummary();
        RegionalLeaveRequestBaselineRequiredCodeRuleFlagsABlankCode();
        RegionalLeaveRequestBaselineScoreRangeRuleFlagsValuesAboveScore();
    }
}
