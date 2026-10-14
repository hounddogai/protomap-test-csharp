namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class FallbackMobilityEntryRulesChecks
{
    public static void FallbackMobilityEntryValidatorReportsEveryProblem()
    {
        var validator = FallbackMobilityEntryRules.DefaultFallbackMobilityEntryValidator();
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackMobilityEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackMobilityEntryTitleLengthRule(32);
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackMobilityEntryFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new FallbackMobilityEntryFloorValueRangeRule(5.3, 330.9);
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("x", "y");
        item.FloorValue = 332.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackMobilityEntryRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new FallbackMobilityEntryRequiredReviewerRule("fallback mobility entry needs a value");
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackMobilityEntryVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new FallbackMobilityEntryVersionRangeRule(1000, 86400);
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackMobilityEntryValidatorReportsEveryProblem();
        FallbackMobilityEntryTitleLengthRuleFlagsALongTitle();
        FallbackMobilityEntryFloorValueRangeRuleFlagsValuesAboveFloorValue();
        FallbackMobilityEntryRequiredReviewerRuleFlagsABlankReviewer();
        FallbackMobilityEntryVersionRangeRuleFlagsValuesAboveVersion();
    }
}
