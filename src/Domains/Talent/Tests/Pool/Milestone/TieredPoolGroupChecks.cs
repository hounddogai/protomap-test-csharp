namespace Acme.Workforce.Talent.Tests.Pool.Milestone;

using Acme.Workforce.Talent.Pool.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TieredPoolGroupChecks
{
    public static void TieredPoolGroupStartsInCreated()
    {
        var item = TieredPoolGroup.NewTieredPoolGroup("code-987", "name-790");
        Check.Equal(item.Status, TieredPoolGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPoolGroupMovesToRetired()
    {
        var item = TieredPoolGroup.NewTieredPoolGroup("code-987", "name-790");
        try
        {
            item.MoveTo(TieredPoolGroupStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPoolGroupStatus.Retired, "status");
    }

    public static void TieredPoolGroupRejectsMovesFromRetired()
    {
        var item = TieredPoolGroup.NewTieredPoolGroup("code-987", "name-790");
        item.Status = TieredPoolGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredPoolGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredPoolGroupStartsInCreated();
        TieredPoolGroupMovesToRetired();
        TieredPoolGroupRejectsMovesFromRetired();
    }
}
