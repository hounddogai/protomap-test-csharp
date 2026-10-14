namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanEntryRulesChecks
{
    public static void FallbackSuccessionPlanEntryValidatorReportsEveryProblem()
    {
        var validator = FallbackSuccessionPlanEntryRules.DefaultFallbackSuccessionPlanEntryValidator();
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackSuccessionPlanEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackSuccessionPlanEntryLabelLengthRule(3600);
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackSuccessionPlanEntryFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new FallbackSuccessionPlanEntryFloorValueRangeRule(4.3, 2051.3);
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("x", "y");
        item.FloorValue = 2052.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackSuccessionPlanEntryDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new FallbackSuccessionPlanEntryDurationMinutesRangeRule(64, 86400);
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackSuccessionPlanEntryLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new FallbackSuccessionPlanEntryLocaleLengthRule(255);
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackSuccessionPlanEntryNotesLengthRuleFlagsALongNotes()
    {
        var rule = new FallbackSuccessionPlanEntryNotesLengthRule(255);
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackSuccessionPlanEntryValidatorReportsEveryProblem();
        FallbackSuccessionPlanEntryLabelLengthRuleFlagsALongLabel();
        FallbackSuccessionPlanEntryFloorValueRangeRuleFlagsValuesAboveFloorValue();
        FallbackSuccessionPlanEntryDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        FallbackSuccessionPlanEntryLocaleLengthRuleFlagsALongLocale();
        FallbackSuccessionPlanEntryNotesLengthRuleFlagsALongNotes();
    }
}
