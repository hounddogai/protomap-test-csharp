namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PoolRequestChecks
{
    public static void PoolRequestStartsInNew()
    {
        var item = PoolRequest.NewPoolRequest("code-673", "name-19");
        Check.Equal(item.Status, PoolRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolRequestMovesToRunning()
    {
        var item = PoolRequest.NewPoolRequest("code-673", "name-19");
        try
        {
            item.MoveTo(PoolRequestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolRequestStatus.Running, "status");
    }

    public static void PoolRequestRejectsMovesFromFailed()
    {
        var item = PoolRequest.NewPoolRequest("code-673", "name-19");
        item.Status = PoolRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(PoolRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        PoolRequestStartsInNew();
        PoolRequestMovesToRunning();
        PoolRequestRejectsMovesFromFailed();
    }
}
