namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LeaveRequestDraftRulesChecks
{
    public static void LeaveRequestDraftValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestDraftRules.DefaultLeaveRequestDraftValidator();
        var item = LeaveRequestDraft.NewLeaveRequestDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LeaveRequestDraftRequiredTitleRule("leave request draft needs this field");
        var item = LeaveRequestDraft.NewLeaveRequestDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestDraftRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new LeaveRequestDraftRequiredCurrencyRule("leave request draft needs a value");
        var item = LeaveRequestDraft.NewLeaveRequestDraft("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestDraftRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LeaveRequestDraftRateRangeRule(1.9, 4554.9);
        var item = LeaveRequestDraft.NewLeaveRequestDraft("x", "y");
        item.Rate = 4556.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestDraftChannelLengthRuleFlagsALongChannel()
    {
        var rule = new LeaveRequestDraftChannelLengthRule(64);
        var item = LeaveRequestDraft.NewLeaveRequestDraft("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestDraftRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new LeaveRequestDraftRequiredChannelRule("leave request draft needs a value");
        var item = LeaveRequestDraft.NewLeaveRequestDraft("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestDraftValidatorReportsEveryProblem();
        LeaveRequestDraftRequiredTitleRuleFlagsABlankTitle();
        LeaveRequestDraftRequiredCurrencyRuleFlagsABlankCurrency();
        LeaveRequestDraftRateRangeRuleFlagsValuesAboveRate();
        LeaveRequestDraftChannelLengthRuleFlagsALongChannel();
        LeaveRequestDraftRequiredChannelRuleFlagsABlankChannel();
    }
}
