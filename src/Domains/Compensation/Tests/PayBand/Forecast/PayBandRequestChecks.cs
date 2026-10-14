namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class PayBandRequestChecks
{
    public static void PayBandRequestStartsInPlanned()
    {
        var item = PayBandRequest.NewPayBandRequest("code-593", "name-25");
        Check.Equal(item.Status, PayBandRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandRequestMovesToCompleted()
    {
        var item = PayBandRequest.NewPayBandRequest("code-593", "name-25");
        try
        {
            item.MoveTo(PayBandRequestStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandRequestStatus.Completed, "status");
    }

    public static void PayBandRequestRejectsMovesFromCancelled()
    {
        var item = PayBandRequest.NewPayBandRequest("code-593", "name-25");
        item.Status = PayBandRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PayBandRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PayBandRequestStartsInPlanned();
        PayBandRequestMovesToCompleted();
        PayBandRequestRejectsMovesFromCancelled();
    }
}
