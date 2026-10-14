namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PrimaryRateRequestChecks
{
    public static void PrimaryRateRequestStartsInCreated()
    {
        var item = PrimaryRateRequest.NewPrimaryRateRequest("code-878", "label-35");
        Check.Equal(item.Status, PrimaryRateRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRateRequestMovesToRetired()
    {
        var item = PrimaryRateRequest.NewPrimaryRateRequest("code-878", "label-35");
        try
        {
            item.MoveTo(PrimaryRateRequestStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRateRequestStatus.Retired, "status");
    }

    public static void PrimaryRateRequestRejectsMovesFromRetired()
    {
        var item = PrimaryRateRequest.NewPrimaryRateRequest("code-878", "label-35");
        item.Status = PrimaryRateRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryRateRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryRateRequestStartsInCreated();
        PrimaryRateRequestMovesToRetired();
        PrimaryRateRequestRejectsMovesFromRetired();
    }
}
