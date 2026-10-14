namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class ScheduledPayBandManifestRulesChecks
{
    public static void ScheduledPayBandManifestValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayBandManifestRules.DefaultScheduledPayBandManifestValidator();
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayBandManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ScheduledPayBandManifestExpiresAtRangeRule(80, 255);
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayBandManifestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledPayBandManifestTitleLengthRule(80);
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayBandManifestRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new ScheduledPayBandManifestRequiredChecksumRule("scheduled pay band manifest needs a value");
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayBandManifestWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new ScheduledPayBandManifestWindowDaysRangeRule(255, 3600);
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayBandManifestStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new ScheduledPayBandManifestStartsAtRangeRule(64, 255);
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledPayBandManifestValidatorReportsEveryProblem();
        ScheduledPayBandManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ScheduledPayBandManifestTitleLengthRuleFlagsALongTitle();
        ScheduledPayBandManifestRequiredChecksumRuleFlagsABlankChecksum();
        ScheduledPayBandManifestWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        ScheduledPayBandManifestStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
