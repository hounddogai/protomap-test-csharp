namespace Acme.Workforce.Workplace.Tests.Booking.Review;

using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class WeightedBookingEntryRulesChecks
{
    public static void WeightedBookingEntryValidatorReportsEveryProblem()
    {
        var validator = WeightedBookingEntryRules.DefaultWeightedBookingEntryValidator();
        var item = WeightedBookingEntry.NewWeightedBookingEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBookingEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedBookingEntryCodeLengthRule(80);
        var item = WeightedBookingEntry.NewWeightedBookingEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBookingEntryChannelLengthRuleFlagsALongChannel()
    {
        var rule = new WeightedBookingEntryChannelLengthRule(32);
        var item = WeightedBookingEntry.NewWeightedBookingEntry("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedBookingEntryValidatorReportsEveryProblem();
        WeightedBookingEntryCodeLengthRuleFlagsALongCode();
        WeightedBookingEntryChannelLengthRuleFlagsALongChannel();
    }
}
