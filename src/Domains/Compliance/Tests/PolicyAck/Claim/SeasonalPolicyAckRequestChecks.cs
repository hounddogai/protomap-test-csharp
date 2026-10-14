namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SeasonalPolicyAckRequestChecks
{
    public static void SeasonalPolicyAckRequestStartsInRequested()
    {
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("code-968", "label-120");
        Check.Equal(item.Status, SeasonalPolicyAckRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPolicyAckRequestMovesToExpired()
    {
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("code-968", "label-120");
        try
        {
            item.MoveTo(SeasonalPolicyAckRequestStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPolicyAckRequestStatus.Expired, "status");
    }

    public static void SeasonalPolicyAckRequestRejectsMovesFromExpired()
    {
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("code-968", "label-120");
        item.Status = SeasonalPolicyAckRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalPolicyAckRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalPolicyAckRequestStartsInRequested();
        SeasonalPolicyAckRequestMovesToExpired();
        SeasonalPolicyAckRequestRejectsMovesFromExpired();
    }
}
