namespace Acme.Workforce.Contractors.Tests.Rate.Request;

using Acme.Workforce.Contractors.Rate.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RateRequestChecks
{
    public static void RateRequestStartsInRequested()
    {
        var item = RateRequest.NewRateRequest("code-549", "label-933");
        Check.Equal(item.Status, RateRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateRequestMovesToGranted()
    {
        var item = RateRequest.NewRateRequest("code-549", "label-933");
        try
        {
            item.MoveTo(RateRequestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateRequestStatus.Granted, "status");
    }

    public static void RateRequestRejectsMovesFromExpired()
    {
        var item = RateRequest.NewRateRequest("code-549", "label-933");
        item.Status = RateRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(RateRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RateRequestStartsInRequested();
        RateRequestMovesToGranted();
        RateRequestRejectsMovesFromExpired();
    }
}
