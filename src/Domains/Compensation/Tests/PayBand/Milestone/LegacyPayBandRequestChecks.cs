namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LegacyPayBandRequestChecks
{
    public static void LegacyPayBandRequestStartsInOpen()
    {
        var item = LegacyPayBandRequest.NewLegacyPayBandRequest("code-601", "name-673");
        Check.Equal(item.Status, LegacyPayBandRequestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayBandRequestMovesToArchived()
    {
        var item = LegacyPayBandRequest.NewLegacyPayBandRequest("code-601", "name-673");
        try
        {
            item.MoveTo(LegacyPayBandRequestStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayBandRequestStatus.Archived, "status");
    }

    public static void LegacyPayBandRequestRejectsMovesFromArchived()
    {
        var item = LegacyPayBandRequest.NewLegacyPayBandRequest("code-601", "name-673");
        item.Status = LegacyPayBandRequestStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyPayBandRequestStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyPayBandRequestStartsInOpen();
        LegacyPayBandRequestMovesToArchived();
        LegacyPayBandRequestRejectsMovesFromArchived();
    }
}
