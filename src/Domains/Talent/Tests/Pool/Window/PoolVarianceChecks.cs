namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolVarianceChecks
{
    public static void PoolVarianceStartsInCreated()
    {
        var item = PoolVariance.NewPoolVariance("code-534", "label-899");
        Check.Equal(item.Status, PoolVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolVarianceMovesToValidated()
    {
        var item = PoolVariance.NewPoolVariance("code-534", "label-899");
        try
        {
            item.MoveTo(PoolVarianceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolVarianceStatus.Validated, "status");
    }

    public static void PoolVarianceRejectsMovesFromRetired()
    {
        var item = PoolVariance.NewPoolVariance("code-534", "label-899");
        item.Status = PoolVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(PoolVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        PoolVarianceStartsInCreated();
        PoolVarianceMovesToValidated();
        PoolVarianceRejectsMovesFromRetired();
    }
}
