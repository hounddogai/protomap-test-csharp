namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class FallbackMobilityEntryChecks
{
    public static void FallbackMobilityEntryStartsInPlanned()
    {
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("code-87", "title-682");
        Check.Equal(item.Status, FallbackMobilityEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackMobilityEntryMovesToCompleted()
    {
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("code-87", "title-682");
        try
        {
            item.MoveTo(FallbackMobilityEntryStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackMobilityEntryStatus.Completed, "status");
    }

    public static void FallbackMobilityEntryRejectsMovesFromCancelled()
    {
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("code-87", "title-682");
        item.Status = FallbackMobilityEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackMobilityEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackMobilityEntryStartsInPlanned();
        FallbackMobilityEntryMovesToCompleted();
        FallbackMobilityEntryRejectsMovesFromCancelled();
    }
}
