namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedVisitorEntryChecks
{
    public static void WeightedVisitorEntryStartsInRequested()
    {
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("code-953", "name-789");
        Check.Equal(item.Status, WeightedVisitorEntryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedVisitorEntryMovesToGranted()
    {
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("code-953", "name-789");
        try
        {
            item.MoveTo(WeightedVisitorEntryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedVisitorEntryStatus.Granted, "status");
    }

    public static void WeightedVisitorEntryRejectsMovesFromExpired()
    {
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("code-953", "name-789");
        item.Status = WeightedVisitorEntryStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedVisitorEntryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedVisitorEntryStartsInRequested();
        WeightedVisitorEntryMovesToGranted();
        WeightedVisitorEntryRejectsMovesFromExpired();
    }
}
