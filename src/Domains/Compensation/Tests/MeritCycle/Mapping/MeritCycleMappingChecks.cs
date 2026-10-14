namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class MeritCycleMappingChecks
{
    public static void MeritCycleMappingStartsInDraft()
    {
        var item = MeritCycleMapping.NewMeritCycleMapping("code-48", "title-384");
        Check.Equal(item.Status, MeritCycleMappingStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleMappingMovesToSuspended()
    {
        var item = MeritCycleMapping.NewMeritCycleMapping("code-48", "title-384");
        try
        {
            item.MoveTo(MeritCycleMappingStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleMappingStatus.Suspended, "status");
    }

    public static void MeritCycleMappingRejectsMovesFromClosed()
    {
        var item = MeritCycleMapping.NewMeritCycleMapping("code-48", "title-384");
        item.Status = MeritCycleMappingStatus.Closed;
        Check.Fails(() => item.MoveTo(MeritCycleMappingStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MeritCycleMappingStartsInDraft();
        MeritCycleMappingMovesToSuspended();
        MeritCycleMappingRejectsMovesFromClosed();
    }
}
