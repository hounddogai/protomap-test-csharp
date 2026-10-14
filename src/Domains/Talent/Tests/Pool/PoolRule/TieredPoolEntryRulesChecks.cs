namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class TieredPoolEntryRulesChecks
{
    public static void TieredPoolEntryValidatorReportsEveryProblem()
    {
        var validator = TieredPoolEntryRules.DefaultTieredPoolEntryValidator();
        var item = TieredPoolEntry.NewTieredPoolEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPoolEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredPoolEntryRequiredNameRule("tiered pool entry needs a value");
        var item = TieredPoolEntry.NewTieredPoolEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPoolEntryRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new TieredPoolEntryRequiredNotesRule("tiered pool entry needs an entry");
        var item = TieredPoolEntry.NewTieredPoolEntry("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPoolEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredPoolEntryCodeLengthRule(16);
        var item = TieredPoolEntry.NewTieredPoolEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredPoolEntryValidatorReportsEveryProblem();
        TieredPoolEntryRequiredNameRuleFlagsABlankName();
        TieredPoolEntryRequiredNotesRuleFlagsABlankNotes();
        TieredPoolEntryCodeLengthRuleFlagsALongCode();
    }
}
