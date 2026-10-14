namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ReorgEntryChecks
{
    public static void ReorgEntryStartsInCreated()
    {
        var item = ReorgEntry.NewReorgEntry("code-81", "name-175");
        Check.Equal(item.Status, ReorgEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgEntryMovesToPublished()
    {
        var item = ReorgEntry.NewReorgEntry("code-81", "name-175");
        try
        {
            item.MoveTo(ReorgEntryStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgEntryStatus.Published, "status");
    }

    public static void ReorgEntryRejectsMovesFromRetired()
    {
        var item = ReorgEntry.NewReorgEntry("code-81", "name-175");
        item.Status = ReorgEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(ReorgEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReorgEntryStartsInCreated();
        ReorgEntryMovesToPublished();
        ReorgEntryRejectsMovesFromRetired();
    }
}
