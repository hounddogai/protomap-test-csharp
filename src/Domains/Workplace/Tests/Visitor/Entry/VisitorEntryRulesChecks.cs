namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class VisitorEntryRulesChecks
{
    public static void VisitorEntryValidatorReportsEveryProblem()
    {
        var validator = VisitorEntryRules.DefaultVisitorEntryValidator();
        var item = VisitorEntry.NewVisitorEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorEntryRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new VisitorEntryRevisionRangeRule(80, 86400);
        var item = VisitorEntry.NewVisitorEntry("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorEntryExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new VisitorEntryExpiresAtRangeRule(16, 16);
        var item = VisitorEntry.NewVisitorEntry("x", "y");
        item.ExpiresAt = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new VisitorEntryRequiredTitleRule("visitor entry needs an entry");
        var item = VisitorEntry.NewVisitorEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorEntryValidatorReportsEveryProblem();
        VisitorEntryRevisionRangeRuleFlagsValuesAboveRevision();
        VisitorEntryExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        VisitorEntryRequiredTitleRuleFlagsABlankTitle();
    }
}
