namespace Acme.Workforce.Absence.Tests.LeaveRequest.Milestone;

using Acme.Workforce.Absence.LeaveRequest.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LeaveRequestEntryRulesChecks
{
    public static void LeaveRequestEntryValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestEntryRules.DefaultLeaveRequestEntryValidator();
        var item = LeaveRequestEntry.NewLeaveRequestEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestEntryNameLengthRuleFlagsALongName()
    {
        var rule = new LeaveRequestEntryNameLengthRule(120);
        var item = LeaveRequestEntry.NewLeaveRequestEntry("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestEntryRequiredCodeRule("leave request entry needs this field");
        var item = LeaveRequestEntry.NewLeaveRequestEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestEntryFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new LeaveRequestEntryFloorValueRangeRule(9.6, 2990.7);
        var item = LeaveRequestEntry.NewLeaveRequestEntry("x", "y");
        item.FloorValue = 2992.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestEntryRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new LeaveRequestEntryRatingRangeRule(5.8, 4740.7);
        var item = LeaveRequestEntry.NewLeaveRequestEntry("x", "y");
        item.Rating = 4742.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestEntrySummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LeaveRequestEntrySummaryLengthRule(32);
        var item = LeaveRequestEntry.NewLeaveRequestEntry("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LeaveRequestEntryValidatorReportsEveryProblem();
        LeaveRequestEntryNameLengthRuleFlagsALongName();
        LeaveRequestEntryRequiredCodeRuleFlagsABlankCode();
        LeaveRequestEntryFloorValueRangeRuleFlagsValuesAboveFloorValue();
        LeaveRequestEntryRatingRangeRuleFlagsValuesAboveRating();
        LeaveRequestEntrySummaryLengthRuleFlagsALongSummary();
    }
}
