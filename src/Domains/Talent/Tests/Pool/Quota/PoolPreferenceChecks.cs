namespace Acme.Workforce.Talent.Tests.Pool.Quota;

using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PoolPreferenceChecks
{
    public static void PoolPreferenceStartsInOpen()
    {
        var item = PoolPreference.NewPoolPreference("code-581", "label-944");
        Check.Equal(item.Status, PoolPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolPreferenceMovesToReopened()
    {
        var item = PoolPreference.NewPoolPreference("code-581", "label-944");
        try
        {
            item.MoveTo(PoolPreferenceStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolPreferenceStatus.Reopened, "status");
    }

    public static void PoolPreferenceRejectsMovesFromArchived()
    {
        var item = PoolPreference.NewPoolPreference("code-581", "label-944");
        item.Status = PoolPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(PoolPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        PoolPreferenceStartsInOpen();
        PoolPreferenceMovesToReopened();
        PoolPreferenceRejectsMovesFromArchived();
    }
}
