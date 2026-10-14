namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityRecordChecks
{
    public static void MobilityRecordStartsInDraft()
    {
        var item = MobilityRecord.NewMobilityRecord("code-549", "title-859");
        Check.Equal(item.Status, MobilityRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityRecordMovesToActive()
    {
        var item = MobilityRecord.NewMobilityRecord("code-549", "title-859");
        try
        {
            item.MoveTo(MobilityRecordStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityRecordStatus.Active, "status");
    }

    public static void MobilityRecordRejectsMovesFromClosed()
    {
        var item = MobilityRecord.NewMobilityRecord("code-549", "title-859");
        item.Status = MobilityRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(MobilityRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MobilityRecordStartsInDraft();
        MobilityRecordMovesToActive();
        MobilityRecordRejectsMovesFromClosed();
    }
}
