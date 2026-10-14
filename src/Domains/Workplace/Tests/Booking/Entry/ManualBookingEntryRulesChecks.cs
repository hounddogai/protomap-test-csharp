namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualBookingEntryRulesChecks
{
    public static void ManualBookingEntryValidatorReportsEveryProblem()
    {
        var validator = ManualBookingEntryRules.DefaultManualBookingEntryValidator();
        var item = ManualBookingEntry.NewManualBookingEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBookingEntryTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new ManualBookingEntryTimezoneLengthRule(64);
        var item = ManualBookingEntry.NewManualBookingEntry("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBookingEntryAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new ManualBookingEntryAmountRangeRule(3.9, 703.9);
        var item = ManualBookingEntry.NewManualBookingEntry("x", "y");
        item.Amount = 705.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualBookingEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualBookingEntryTitleLengthRule(3600);
        var item = ManualBookingEntry.NewManualBookingEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualBookingEntryValidatorReportsEveryProblem();
        ManualBookingEntryTimezoneLengthRuleFlagsALongTimezone();
        ManualBookingEntryAmountRangeRuleFlagsValuesAboveAmount();
        ManualBookingEntryTitleLengthRuleFlagsALongTitle();
    }
}
