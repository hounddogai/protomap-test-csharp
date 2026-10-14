namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandScheduleChecks
{
    public static void PayBandScheduleStartsInOpen()
    {
        var item = PayBandSchedule.NewPayBandSchedule("code-40", "label-525");
        Check.Equal(item.Status, PayBandScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandScheduleMovesToResolved()
    {
        var item = PayBandSchedule.NewPayBandSchedule("code-40", "label-525");
        try
        {
            item.MoveTo(PayBandScheduleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandScheduleStatus.Resolved, "status");
    }

    public static void PayBandScheduleRejectsMovesFromArchived()
    {
        var item = PayBandSchedule.NewPayBandSchedule("code-40", "label-525");
        item.Status = PayBandScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(PayBandScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        PayBandScheduleStartsInOpen();
        PayBandScheduleMovesToResolved();
        PayBandScheduleRejectsMovesFromArchived();
    }
}
