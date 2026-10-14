namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandManifestChecks
{
    public static void PayBandManifestStartsInOpen()
    {
        var item = PayBandManifest.NewPayBandManifest("code-640", "title-798");
        Check.Equal(item.Status, PayBandManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandManifestMovesToArchived()
    {
        var item = PayBandManifest.NewPayBandManifest("code-640", "title-798");
        try
        {
            item.MoveTo(PayBandManifestStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandManifestStatus.Archived, "status");
    }

    public static void PayBandManifestRejectsMovesFromArchived()
    {
        var item = PayBandManifest.NewPayBandManifest("code-640", "title-798");
        item.Status = PayBandManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(PayBandManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        PayBandManifestStartsInOpen();
        PayBandManifestMovesToArchived();
        PayBandManifestRejectsMovesFromArchived();
    }
}
