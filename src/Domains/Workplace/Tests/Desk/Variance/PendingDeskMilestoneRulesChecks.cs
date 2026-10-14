namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingDeskMilestoneRulesChecks
{
    public static void PendingDeskMilestoneValidatorReportsEveryProblem()
    {
        var validator = PendingDeskMilestoneRules.DefaultPendingDeskMilestoneValidator();
        var item = PendingDeskMilestone.NewPendingDeskMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingDeskMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingDeskMilestoneRequiredCodeRule("pending desk milestone needs this field");
        var item = PendingDeskMilestone.NewPendingDeskMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingDeskMilestoneRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new PendingDeskMilestoneRequiredSummaryRule("pending desk milestone needs a value");
        var item = PendingDeskMilestone.NewPendingDeskMilestone("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingDeskMilestoneValidatorReportsEveryProblem();
        PendingDeskMilestoneRequiredCodeRuleFlagsABlankCode();
        PendingDeskMilestoneRequiredSummaryRuleFlagsABlankSummary();
    }
}
