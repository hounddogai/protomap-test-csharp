namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandTargetChecks
{
    public static void PayBandTargetStartsInPlanned()
    {
        var item = PayBandTarget.NewPayBandTarget("code-551", "name-611");
        Check.Equal(item.Status, PayBandTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandTargetMovesToCompleted()
    {
        var item = PayBandTarget.NewPayBandTarget("code-551", "name-611");
        try
        {
            item.MoveTo(PayBandTargetStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandTargetStatus.Completed, "status");
    }

    public static void PayBandTargetRejectsMovesFromCancelled()
    {
        var item = PayBandTarget.NewPayBandTarget("code-551", "name-611");
        item.Status = PayBandTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PayBandTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PayBandTargetStartsInPlanned();
        PayBandTargetMovesToCompleted();
        PayBandTargetRejectsMovesFromCancelled();
    }
}
