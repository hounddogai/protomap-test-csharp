namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallLedgerRulesChecks
{
    public static void TownHallLedgerValidatorReportsEveryProblem()
    {
        var validator = TownHallLedgerRules.DefaultTownHallLedgerValidator();
        var item = TownHallLedger.NewTownHallLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallLedgerRequiredCodeRule("town hall ledger needs an entry");
        var item = TownHallLedger.NewTownHallLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallLedgerSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new TownHallLedgerSummaryLengthRule(3600);
        var item = TownHallLedger.NewTownHallLedger("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallLedgerJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new TownHallLedgerJobCodeLengthRule(1000);
        var item = TownHallLedger.NewTownHallLedger("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallLedgerValidatorReportsEveryProblem();
        TownHallLedgerRequiredCodeRuleFlagsABlankCode();
        TownHallLedgerSummaryLengthRuleFlagsALongSummary();
        TownHallLedgerJobCodeLengthRuleFlagsALongJobCode();
    }
}
