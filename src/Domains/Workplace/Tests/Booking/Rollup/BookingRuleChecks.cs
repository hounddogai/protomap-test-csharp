namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRuleChecks
{
    public static void BookingRuleStartsInPlanned()
    {
        var item = BookingRule.NewBookingRule("code-135", "name-449");
        Check.Equal(item.Status, BookingRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingRuleMovesToInProgress()
    {
        var item = BookingRule.NewBookingRule("code-135", "name-449");
        try
        {
            item.MoveTo(BookingRuleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingRuleStatus.InProgress, "status");
    }

    public static void BookingRuleRejectsMovesFromCancelled()
    {
        var item = BookingRule.NewBookingRule("code-135", "name-449");
        item.Status = BookingRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BookingRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BookingRuleStartsInPlanned();
        BookingRuleMovesToInProgress();
        BookingRuleRejectsMovesFromCancelled();
    }
}
