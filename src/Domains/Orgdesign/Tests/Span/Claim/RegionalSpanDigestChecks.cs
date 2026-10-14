namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSpanDigestChecks
{
    public static void RegionalSpanDigestStartsInRequested()
    {
        var item = RegionalSpanDigest.NewRegionalSpanDigest("code-45", "label-478");
        Check.Equal(item.Status, RegionalSpanDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSpanDigestMovesToRevoked()
    {
        var item = RegionalSpanDigest.NewRegionalSpanDigest("code-45", "label-478");
        try
        {
            item.MoveTo(RegionalSpanDigestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSpanDigestStatus.Revoked, "status");
    }

    public static void RegionalSpanDigestRejectsMovesFromExpired()
    {
        var item = RegionalSpanDigest.NewRegionalSpanDigest("code-45", "label-478");
        item.Status = RegionalSpanDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalSpanDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalSpanDigestStartsInRequested();
        RegionalSpanDigestMovesToRevoked();
        RegionalSpanDigestRejectsMovesFromExpired();
    }
}
