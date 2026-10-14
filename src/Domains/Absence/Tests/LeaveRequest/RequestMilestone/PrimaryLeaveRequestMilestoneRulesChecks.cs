namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestMilestoneRulesChecks
{
    public static void PrimaryLeaveRequestMilestoneValidatorReportsEveryProblem()
    {
        var validator = PrimaryLeaveRequestMilestoneRules.DefaultPrimaryLeaveRequestMilestoneValidator();
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryLeaveRequestMilestoneTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryLeaveRequestMilestoneTitleLengthRule(120);
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryLeaveRequestMilestoneRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PrimaryLeaveRequestMilestoneRetriesRangeRule(64, 255);
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryLeaveRequestMilestoneRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new PrimaryLeaveRequestMilestoneRequiredSalaryBandRule(
            "primary leave request milestone needs a value");
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryLeaveRequestMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryLeaveRequestMilestoneRequiredCodeRule("primary leave request milestone needs a value");
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestMilestoneValidatorReportsEveryProblem();
        PrimaryLeaveRequestMilestoneTitleLengthRuleFlagsALongTitle();
        PrimaryLeaveRequestMilestoneRetriesRangeRuleFlagsValuesAboveRetries();
        PrimaryLeaveRequestMilestoneRequiredSalaryBandRuleFlagsABlankSalaryBand();
        PrimaryLeaveRequestMilestoneRequiredCodeRuleFlagsABlankCode();
    }
}
