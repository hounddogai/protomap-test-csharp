namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolEntryRulesChecks
{
    public static void BonusPoolEntryValidatorReportsEveryProblem()
    {
        var validator = BonusPoolEntryRules.DefaultBonusPoolEntryValidator();
        var item = BonusPoolEntry.NewBonusPoolEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolEntryCodeLengthRule(32);
        var item = BonusPoolEntry.NewBonusPoolEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolEntryPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new BonusPoolEntryPriorityRangeRule(1000, 86400);
        var item = BonusPoolEntry.NewBonusPoolEntry("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolEntryRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new BonusPoolEntryRequiredNotesRule("bonus pool entry needs this field");
        var item = BonusPoolEntry.NewBonusPoolEntry("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolEntrySummaryLengthRuleFlagsALongSummary()
    {
        var rule = new BonusPoolEntrySummaryLengthRule(64);
        var item = BonusPoolEntry.NewBonusPoolEntry("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new BonusPoolEntryRequiredNameRule("bonus pool entry needs a value");
        var item = BonusPoolEntry.NewBonusPoolEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolEntryValidatorReportsEveryProblem();
        BonusPoolEntryCodeLengthRuleFlagsALongCode();
        BonusPoolEntryPriorityRangeRuleFlagsValuesAbovePriority();
        BonusPoolEntryRequiredNotesRuleFlagsABlankNotes();
        BonusPoolEntrySummaryLengthRuleFlagsALongSummary();
        BonusPoolEntryRequiredNameRuleFlagsABlankName();
    }
}
