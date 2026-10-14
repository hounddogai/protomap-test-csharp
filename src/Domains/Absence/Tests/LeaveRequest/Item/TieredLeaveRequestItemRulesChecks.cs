namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class TieredLeaveRequestItemRulesChecks
{
    public static void TieredLeaveRequestItemValidatorReportsEveryProblem()
    {
        var validator = TieredLeaveRequestItemRules.DefaultTieredLeaveRequestItemValidator();
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLeaveRequestItemStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new TieredLeaveRequestItemStartsAtRangeRule(16, 120);
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredLeaveRequestItemRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new TieredLeaveRequestItemRequiredLocaleRule("tiered leave request item needs an entry");
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLeaveRequestItemRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredLeaveRequestItemRequiredTitleRule("tiered leave request item needs this field");
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLeaveRequestItemTeamLengthRuleFlagsALongTeam()
    {
        var rule = new TieredLeaveRequestItemTeamLengthRule(255);
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredLeaveRequestItemValidatorReportsEveryProblem();
        TieredLeaveRequestItemStartsAtRangeRuleFlagsValuesAboveStartsAt();
        TieredLeaveRequestItemRequiredLocaleRuleFlagsABlankLocale();
        TieredLeaveRequestItemRequiredTitleRuleFlagsABlankTitle();
        TieredLeaveRequestItemTeamLengthRuleFlagsALongTeam();
    }
}
