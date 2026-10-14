namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class PoolExceptionChecks
{
    public static void PoolExceptionStartsInNew()
    {
        var item = PoolException.NewPoolException("code-737", "title-296");
        Check.Equal(item.Status, PoolExceptionStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolExceptionMovesToQueued()
    {
        var item = PoolException.NewPoolException("code-737", "title-296");
        try
        {
            item.MoveTo(PoolExceptionStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolExceptionStatus.Queued, "status");
    }

    public static void PoolExceptionRejectsMovesFromFailed()
    {
        var item = PoolException.NewPoolException("code-737", "title-296");
        item.Status = PoolExceptionStatus.Failed;
        Check.Fails(() => item.MoveTo(PoolExceptionStatus.New), "move");
    }

    public static void RunAll()
    {
        PoolExceptionStartsInNew();
        PoolExceptionMovesToQueued();
        PoolExceptionRejectsMovesFromFailed();
    }
}
