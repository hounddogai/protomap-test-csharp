namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class TieredCostModelEntryChecks
{
    public static void TieredCostModelEntryStartsInCreated()
    {
        var item = TieredCostModelEntry.NewTieredCostModelEntry("code-336", "name-76");
        Check.Equal(item.Status, TieredCostModelEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredCostModelEntryMovesToPublished()
    {
        var item = TieredCostModelEntry.NewTieredCostModelEntry("code-336", "name-76");
        try
        {
            item.MoveTo(TieredCostModelEntryStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredCostModelEntryStatus.Published, "status");
    }

    public static void TieredCostModelEntryRejectsMovesFromRetired()
    {
        var item = TieredCostModelEntry.NewTieredCostModelEntry("code-336", "name-76");
        item.Status = TieredCostModelEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredCostModelEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredCostModelEntryStartsInCreated();
        TieredCostModelEntryMovesToPublished();
        TieredCostModelEntryRejectsMovesFromRetired();
    }
}
