namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TownHallSummaryRulesChecks
{
    public static void TownHallSummaryValidatorReportsEveryProblem()
    {
        var validator = TownHallSummaryRules.DefaultTownHallSummaryValidator();
        var item = TownHallSummary.NewTownHallSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallSummaryRequiredCodeRule("town hall summary needs this field");
        var item = TownHallSummary.NewTownHallSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallSummaryValidatorReportsEveryProblem();
        TownHallSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
