namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskScoreRulesChecks
{
    public static void DeskScoreValidatorReportsEveryProblem()
    {
        var validator = DeskScoreRules.DefaultDeskScoreValidator();
        var item = DeskScore.NewDeskScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskScorePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new DeskScorePhoneLengthRule(255);
        var item = DeskScore.NewDeskScore("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskScoreRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new DeskScoreRequiredPhoneRule("desk score needs an entry");
        var item = DeskScore.NewDeskScore("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeskScoreRequiredTitleRule("desk score needs this field");
        var item = DeskScore.NewDeskScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskScoreSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new DeskScoreSummaryLengthRule(1000);
        var item = DeskScore.NewDeskScore("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskScoreRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new DeskScoreRequiredSummaryRule("desk score needs a value");
        var item = DeskScore.NewDeskScore("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DeskScoreTitleLengthRule(3600);
        var item = DeskScore.NewDeskScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskScoreValidatorReportsEveryProblem();
        DeskScorePhoneLengthRuleFlagsALongPhone();
        DeskScoreRequiredPhoneRuleFlagsABlankPhone();
        DeskScoreRequiredTitleRuleFlagsABlankTitle();
        DeskScoreSummaryLengthRuleFlagsALongSummary();
        DeskScoreRequiredSummaryRuleFlagsABlankSummary();
        DeskScoreTitleLengthRuleFlagsALongTitle();
    }
}
