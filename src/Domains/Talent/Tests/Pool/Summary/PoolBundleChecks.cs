namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PoolBundleChecks
{
    public static void PoolBundleStartsInCreated()
    {
        var item = PoolBundle.NewPoolBundle("code-523", "name-854");
        Check.Equal(item.Status, PoolBundleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolBundleMovesToPublished()
    {
        var item = PoolBundle.NewPoolBundle("code-523", "name-854");
        try
        {
            item.MoveTo(PoolBundleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolBundleStatus.Published, "status");
    }

    public static void PoolBundleRejectsMovesFromRetired()
    {
        var item = PoolBundle.NewPoolBundle("code-523", "name-854");
        item.Status = PoolBundleStatus.Retired;
        Check.Fails(() => item.MoveTo(PoolBundleStatus.Created), "move");
    }

    public static void RunAll()
    {
        PoolBundleStartsInCreated();
        PoolBundleMovesToPublished();
        PoolBundleRejectsMovesFromRetired();
    }
}
