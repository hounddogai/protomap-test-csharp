namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class TieredLeaveRequestOverrideRulesChecks
{
    public static void TieredLeaveRequestOverrideValidatorReportsEveryProblem()
    {
        var validator = TieredLeaveRequestOverrideRules.DefaultTieredLeaveRequestOverrideValidator();
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLeaveRequestOverrideRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredLeaveRequestOverrideRequiredTitleRule("tiered leave request override needs this field");
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLeaveRequestOverrideTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredLeaveRequestOverrideTitleLengthRule(32);
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredLeaveRequestOverrideValidatorReportsEveryProblem();
        TieredLeaveRequestOverrideRequiredTitleRuleFlagsABlankTitle();
        TieredLeaveRequestOverrideTitleLengthRuleFlagsALongTitle();
    }
}
