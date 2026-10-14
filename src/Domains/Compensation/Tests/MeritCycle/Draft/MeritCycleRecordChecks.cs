namespace Acme.Workforce.Compensation.Tests.MeritCycle.Draft;

using Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class MeritCycleRecordChecks
{
    public static void MeritCycleRecordStartsInDraft()
    {
        var item = MeritCycleRecord.NewMeritCycleRecord("code-193", "name-322");
        Check.Equal(item.Status, MeritCycleRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleRecordMovesToPending()
    {
        var item = MeritCycleRecord.NewMeritCycleRecord("code-193", "name-322");
        try
        {
            item.MoveTo(MeritCycleRecordStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleRecordStatus.Pending, "status");
    }

    public static void MeritCycleRecordRejectsMovesFromClosed()
    {
        var item = MeritCycleRecord.NewMeritCycleRecord("code-193", "name-322");
        item.Status = MeritCycleRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(MeritCycleRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MeritCycleRecordStartsInDraft();
        MeritCycleRecordMovesToPending();
        MeritCycleRecordRejectsMovesFromClosed();
    }
}
