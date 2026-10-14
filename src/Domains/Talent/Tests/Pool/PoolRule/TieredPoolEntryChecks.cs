namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class TieredPoolEntryChecks
{
    public static void TieredPoolEntryStartsInCreated()
    {
        var item = TieredPoolEntry.NewTieredPoolEntry("code-746", "name-262");
        Check.Equal(item.Status, TieredPoolEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPoolEntryMovesToRetired()
    {
        var item = TieredPoolEntry.NewTieredPoolEntry("code-746", "name-262");
        try
        {
            item.MoveTo(TieredPoolEntryStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPoolEntryStatus.Retired, "status");
    }

    public static void TieredPoolEntryRejectsMovesFromRetired()
    {
        var item = TieredPoolEntry.NewTieredPoolEntry("code-746", "name-262");
        item.Status = TieredPoolEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredPoolEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredPoolEntryStartsInCreated();
        TieredPoolEntryMovesToRetired();
        TieredPoolEntryRejectsMovesFromRetired();
    }
}
