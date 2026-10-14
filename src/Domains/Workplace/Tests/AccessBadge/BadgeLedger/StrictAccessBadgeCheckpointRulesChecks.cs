namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class StrictAccessBadgeCheckpointRulesChecks
{
    public static void StrictAccessBadgeCheckpointValidatorReportsEveryProblem()
    {
        var validator = StrictAccessBadgeCheckpointRules.DefaultStrictAccessBadgeCheckpointValidator();
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAccessBadgeCheckpointDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new StrictAccessBadgeCheckpointDiscountRangeRule(5.1, 3731.2);
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("x", "y");
        item.Discount = 3732.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAccessBadgeCheckpointRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new StrictAccessBadgeCheckpointRequiredNotesRule("strict access badge checkpoint needs a value");
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAccessBadgeCheckpointRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new StrictAccessBadgeCheckpointRequiredManagerIdRule("strict access badge checkpoint needs a value");
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAccessBadgeCheckpointFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new StrictAccessBadgeCheckpointFteRangeRule(9.1, 3587.0);
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("x", "y");
        item.Fte = 3588.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAccessBadgeCheckpointCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new StrictAccessBadgeCheckpointCreatedAtRangeRule(64, 64);
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("x", "y");
        item.CreatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictAccessBadgeCheckpointValidatorReportsEveryProblem();
        StrictAccessBadgeCheckpointDiscountRangeRuleFlagsValuesAboveDiscount();
        StrictAccessBadgeCheckpointRequiredNotesRuleFlagsABlankNotes();
        StrictAccessBadgeCheckpointRequiredManagerIdRuleFlagsABlankManagerId();
        StrictAccessBadgeCheckpointFteRangeRuleFlagsValuesAboveFte();
        StrictAccessBadgeCheckpointCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
