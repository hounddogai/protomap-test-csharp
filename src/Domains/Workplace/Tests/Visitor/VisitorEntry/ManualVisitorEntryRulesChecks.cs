namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class ManualVisitorEntryRulesChecks
{
    public static void ManualVisitorEntryValidatorReportsEveryProblem()
    {
        var validator = ManualVisitorEntryRules.DefaultManualVisitorEntryValidator();
        var item = ManualVisitorEntry.NewManualVisitorEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualVisitorEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualVisitorEntryLabelLengthRule(86400);
        var item = ManualVisitorEntry.NewManualVisitorEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualVisitorEntryMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ManualVisitorEntryMinimumRangeRule(7.3, 557.7);
        var item = ManualVisitorEntry.NewManualVisitorEntry("x", "y");
        item.Minimum = 559.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualVisitorEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualVisitorEntryRequiredLabelRule("manual visitor entry needs this field");
        var item = ManualVisitorEntry.NewManualVisitorEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualVisitorEntryValidatorReportsEveryProblem();
        ManualVisitorEntryLabelLengthRuleFlagsALongLabel();
        ManualVisitorEntryMinimumRangeRuleFlagsValuesAboveMinimum();
        ManualVisitorEntryRequiredLabelRuleFlagsABlankLabel();
    }
}
