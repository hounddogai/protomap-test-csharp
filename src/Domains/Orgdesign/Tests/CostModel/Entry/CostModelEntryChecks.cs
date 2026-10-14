namespace Acme.Workforce.Orgdesign.Tests.CostModel.Entry;

using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CostModelEntryChecks
{
    public static void CostModelEntryStartsInRequested()
    {
        var item = CostModelEntry.NewCostModelEntry("code-334", "name-143");
        Check.Equal(item.Status, CostModelEntryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelEntryMovesToExpired()
    {
        var item = CostModelEntry.NewCostModelEntry("code-334", "name-143");
        try
        {
            item.MoveTo(CostModelEntryStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelEntryStatus.Expired, "status");
    }

    public static void CostModelEntryRejectsMovesFromExpired()
    {
        var item = CostModelEntry.NewCostModelEntry("code-334", "name-143");
        item.Status = CostModelEntryStatus.Expired;
        Check.Fails(() => item.MoveTo(CostModelEntryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CostModelEntryStartsInRequested();
        CostModelEntryMovesToExpired();
        CostModelEntryRejectsMovesFromExpired();
    }
}
