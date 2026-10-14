namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class GlobalMobilityRecordChecks
{
    public static void GlobalMobilityRecordStartsInRequested()
    {
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("code-788", "title-940");
        Check.Equal(item.Status, GlobalMobilityRecordStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalMobilityRecordMovesToRevoked()
    {
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("code-788", "title-940");
        try
        {
            item.MoveTo(GlobalMobilityRecordStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalMobilityRecordStatus.Revoked, "status");
    }

    public static void GlobalMobilityRecordRejectsMovesFromExpired()
    {
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("code-788", "title-940");
        item.Status = GlobalMobilityRecordStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalMobilityRecordStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalMobilityRecordStartsInRequested();
        GlobalMobilityRecordMovesToRevoked();
        GlobalMobilityRecordRejectsMovesFromExpired();
    }
}
