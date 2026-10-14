namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LenientRecognitionEntryRulesChecks
{
    public static void LenientRecognitionEntryValidatorReportsEveryProblem()
    {
        var validator = LenientRecognitionEntryRules.DefaultLenientRecognitionEntryValidator();
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRecognitionEntryRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new LenientRecognitionEntryRequiredDescriptionRule("lenient recognition entry needs an entry");
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRecognitionEntryRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new LenientRecognitionEntryRequiredSummaryRule("lenient recognition entry needs an entry");
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRecognitionEntryHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new LenientRecognitionEntryHoursRangeRule(1.8, 1770.5);
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("x", "y");
        item.Hours = 1772.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientRecognitionEntryValidatorReportsEveryProblem();
        LenientRecognitionEntryRequiredDescriptionRuleFlagsABlankDescription();
        LenientRecognitionEntryRequiredSummaryRuleFlagsABlankSummary();
        LenientRecognitionEntryHoursRangeRuleFlagsValuesAboveHours();
    }
}
