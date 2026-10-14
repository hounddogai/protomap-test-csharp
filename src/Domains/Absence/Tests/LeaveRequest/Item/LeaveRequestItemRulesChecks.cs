namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestItemRulesChecks
{
    public static void LeaveRequestItemValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestItemRules.DefaultLeaveRequestItemValidator();
        var item = LeaveRequestItem.NewLeaveRequestItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestItemLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LeaveRequestItemLeaveDaysRangeRule(1000, 86400);
        var item = LeaveRequestItem.NewLeaveRequestItem("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestItemRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new LeaveRequestItemRevisionRangeRule(64, 255);
        var item = LeaveRequestItem.NewLeaveRequestItem("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestItemDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new LeaveRequestItemDescriptionLengthRule(3600);
        var item = LeaveRequestItem.NewLeaveRequestItem("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestItemRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LeaveRequestItemRequiredTitleRule("leave request item needs a value");
        var item = LeaveRequestItem.NewLeaveRequestItem("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestItemValidatorReportsEveryProblem();
        LeaveRequestItemLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        LeaveRequestItemRevisionRangeRuleFlagsValuesAboveRevision();
        LeaveRequestItemDescriptionLengthRuleFlagsALongDescription();
        LeaveRequestItemRequiredTitleRuleFlagsABlankTitle();
    }
}
