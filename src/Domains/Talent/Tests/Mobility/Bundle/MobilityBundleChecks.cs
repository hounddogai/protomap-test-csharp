namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityBundleChecks
{
    public static void MobilityBundleStartsInRequested()
    {
        var item = MobilityBundle.NewMobilityBundle("code-437", "title-898");
        Check.Equal(item.Status, MobilityBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityBundleMovesToRevoked()
    {
        var item = MobilityBundle.NewMobilityBundle("code-437", "title-898");
        try
        {
            item.MoveTo(MobilityBundleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityBundleStatus.Revoked, "status");
    }

    public static void MobilityBundleRejectsMovesFromExpired()
    {
        var item = MobilityBundle.NewMobilityBundle("code-437", "title-898");
        item.Status = MobilityBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(MobilityBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        MobilityBundleStartsInRequested();
        MobilityBundleMovesToRevoked();
        MobilityBundleRejectsMovesFromExpired();
    }
}
