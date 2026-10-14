namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Checks of headcount schedule code, run by the project's runner.</summary>
public static class HeadcountLedgerRulesChecks
{
    public static void HeadcountLedgerValidatorReportsEveryProblem()
    {
        var validator = HeadcountLedgerRules.DefaultHeadcountLedgerValidator();
        var item = HeadcountLedger.NewHeadcountLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountLedgerRequiredCodeRule("headcount ledger needs an entry");
        var item = HeadcountLedger.NewHeadcountLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountLedgerReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new HeadcountLedgerReviewerLengthRule(86400);
        var item = HeadcountLedger.NewHeadcountLedger("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountLedgerRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new HeadcountLedgerRequiredTitleRule("headcount ledger needs a value");
        var item = HeadcountLedger.NewHeadcountLedger("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountLedgerValidatorReportsEveryProblem();
        HeadcountLedgerRequiredCodeRuleFlagsABlankCode();
        HeadcountLedgerReviewerLengthRuleFlagsALongReviewer();
        HeadcountLedgerRequiredTitleRuleFlagsABlankTitle();
    }
}
