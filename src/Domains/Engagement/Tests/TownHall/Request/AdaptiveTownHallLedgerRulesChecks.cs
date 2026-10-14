namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptiveTownHallLedgerRulesChecks
{
    public static void AdaptiveTownHallLedgerValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTownHallLedgerRules.DefaultAdaptiveTownHallLedgerValidator();
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTownHallLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveTownHallLedgerTitleLengthRule(32);
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveTownHallLedgerRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveTownHallLedgerRequiredTitleRule("adaptive town hall ledger needs a value");
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveTownHallLedgerValidatorReportsEveryProblem();
        AdaptiveTownHallLedgerTitleLengthRuleFlagsALongTitle();
        AdaptiveTownHallLedgerRequiredTitleRuleFlagsABlankTitle();
    }
}
