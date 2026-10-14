namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PositionDigestChecks
{
    public static void PositionDigestStartsInNew()
    {
        var item = PositionDigest.NewPositionDigest("code-587", "name-354");
        Check.Equal(item.Status, PositionDigestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionDigestMovesToQueued()
    {
        var item = PositionDigest.NewPositionDigest("code-587", "name-354");
        try
        {
            item.MoveTo(PositionDigestStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionDigestStatus.Queued, "status");
    }

    public static void PositionDigestRejectsMovesFromFailed()
    {
        var item = PositionDigest.NewPositionDigest("code-587", "name-354");
        item.Status = PositionDigestStatus.Failed;
        Check.Fails(() => item.MoveTo(PositionDigestStatus.New), "move");
    }

    public static void RunAll()
    {
        PositionDigestStartsInNew();
        PositionDigestMovesToQueued();
        PositionDigestRejectsMovesFromFailed();
    }
}
