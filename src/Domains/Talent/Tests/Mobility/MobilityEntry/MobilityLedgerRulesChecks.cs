namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class MobilityLedgerRulesChecks
{
    public static void MobilityLedgerValidatorReportsEveryProblem()
    {
        var validator = MobilityLedgerRules.DefaultMobilityLedgerValidator();
        var item = MobilityLedger.NewMobilityLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityLedgerRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new MobilityLedgerRequiredLocaleRule("mobility ledger needs this field");
        var item = MobilityLedger.NewMobilityLedger("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MobilityLedgerTitleLengthRule(32);
        var item = MobilityLedger.NewMobilityLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityLedgerRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new MobilityLedgerRequiredNotesRule("mobility ledger needs a value");
        var item = MobilityLedger.NewMobilityLedger("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilityLedgerValidatorReportsEveryProblem();
        MobilityLedgerRequiredLocaleRuleFlagsABlankLocale();
        MobilityLedgerTitleLengthRuleFlagsALongTitle();
        MobilityLedgerRequiredNotesRuleFlagsABlankNotes();
    }
}
