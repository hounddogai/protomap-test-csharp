namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateManifestChecks
{
    public static void RateManifestStartsInOpen()
    {
        var item = RateManifest.NewRateManifest("code-863", "label-817");
        Check.Equal(item.Status, RateManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateManifestMovesToReopened()
    {
        var item = RateManifest.NewRateManifest("code-863", "label-817");
        try
        {
            item.MoveTo(RateManifestStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateManifestStatus.Reopened, "status");
    }

    public static void RateManifestRejectsMovesFromArchived()
    {
        var item = RateManifest.NewRateManifest("code-863", "label-817");
        item.Status = RateManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(RateManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        RateManifestStartsInOpen();
        RateManifestMovesToReopened();
        RateManifestRejectsMovesFromArchived();
    }
}
