namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountExceptionChecks
{
    public static void HeadcountExceptionStartsInIdle()
    {
        var item = HeadcountException.NewHeadcountException("code-229", "name-155");
        Check.Equal(item.Status, HeadcountExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountExceptionMovesToWarming()
    {
        var item = HeadcountException.NewHeadcountException("code-229", "name-155");
        try
        {
            item.MoveTo(HeadcountExceptionStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountExceptionStatus.Warming, "status");
    }

    public static void HeadcountExceptionRejectsMovesFromStopped()
    {
        var item = HeadcountException.NewHeadcountException("code-229", "name-155");
        item.Status = HeadcountExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(HeadcountExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HeadcountExceptionStartsInIdle();
        HeadcountExceptionMovesToWarming();
        HeadcountExceptionRejectsMovesFromStopped();
    }
}
