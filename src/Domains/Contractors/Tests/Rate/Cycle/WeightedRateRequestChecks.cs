namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class WeightedRateRequestChecks
{
    public static void WeightedRateRequestStartsInDraft()
    {
        var item = WeightedRateRequest.NewWeightedRateRequest("code-71", "title-999");
        Check.Equal(item.Status, WeightedRateRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRateRequestMovesToSuspended()
    {
        var item = WeightedRateRequest.NewWeightedRateRequest("code-71", "title-999");
        try
        {
            item.MoveTo(WeightedRateRequestStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRateRequestStatus.Suspended, "status");
    }

    public static void WeightedRateRequestRejectsMovesFromClosed()
    {
        var item = WeightedRateRequest.NewWeightedRateRequest("code-71", "title-999");
        item.Status = WeightedRateRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedRateRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedRateRequestStartsInDraft();
        WeightedRateRequestMovesToSuspended();
        WeightedRateRequestRejectsMovesFromClosed();
    }
}
