namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientMobilityRequestChecks
{
    public static void LenientMobilityRequestStartsInRequested()
    {
        var item = LenientMobilityRequest.NewLenientMobilityRequest("code-775", "name-923");
        Check.Equal(item.Status, LenientMobilityRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientMobilityRequestMovesToRevoked()
    {
        var item = LenientMobilityRequest.NewLenientMobilityRequest("code-775", "name-923");
        try
        {
            item.MoveTo(LenientMobilityRequestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientMobilityRequestStatus.Revoked, "status");
    }

    public static void LenientMobilityRequestRejectsMovesFromExpired()
    {
        var item = LenientMobilityRequest.NewLenientMobilityRequest("code-775", "name-923");
        item.Status = LenientMobilityRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientMobilityRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientMobilityRequestStartsInRequested();
        LenientMobilityRequestMovesToRevoked();
        LenientMobilityRequestRejectsMovesFromExpired();
    }
}
