namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredRateRequestChecks
{
    public static void TieredRateRequestStartsInPlanned()
    {
        var item = TieredRateRequest.NewTieredRateRequest("code-890", "label-258");
        Check.Equal(item.Status, TieredRateRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredRateRequestMovesToScheduled()
    {
        var item = TieredRateRequest.NewTieredRateRequest("code-890", "label-258");
        try
        {
            item.MoveTo(TieredRateRequestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredRateRequestStatus.Scheduled, "status");
    }

    public static void TieredRateRequestRejectsMovesFromCancelled()
    {
        var item = TieredRateRequest.NewTieredRateRequest("code-890", "label-258");
        item.Status = TieredRateRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredRateRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredRateRequestStartsInPlanned();
        TieredRateRequestMovesToScheduled();
        TieredRateRequestRejectsMovesFromCancelled();
    }
}
