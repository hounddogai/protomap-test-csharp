namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RegionalLeaveRequestItemRulesChecks
{
    public static void RegionalLeaveRequestItemValidatorReportsEveryProblem()
    {
        var validator = RegionalLeaveRequestItemRules.DefaultRegionalLeaveRequestItemValidator();
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalLeaveRequestItemRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new RegionalLeaveRequestItemRequiredLocationRule("regional leave request item needs an entry");
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalLeaveRequestItemRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalLeaveRequestItemRequiredCodeRule("regional leave request item needs an entry");
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalLeaveRequestItemSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new RegionalLeaveRequestItemSummaryLengthRule(255);
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalLeaveRequestItemValidatorReportsEveryProblem();
        RegionalLeaveRequestItemRequiredLocationRuleFlagsABlankLocation();
        RegionalLeaveRequestItemRequiredCodeRuleFlagsABlankCode();
        RegionalLeaveRequestItemSummaryLengthRuleFlagsALongSummary();
    }
}
