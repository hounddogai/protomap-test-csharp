namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedBalanceRequestChecks
{
    public static void CappedBalanceRequestStartsInNew()
    {
        var item = CappedBalanceRequest.NewCappedBalanceRequest("code-716", "title-655");
        Check.Equal(item.Status, CappedBalanceRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedBalanceRequestMovesToRunning()
    {
        var item = CappedBalanceRequest.NewCappedBalanceRequest("code-716", "title-655");
        try
        {
            item.MoveTo(CappedBalanceRequestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedBalanceRequestStatus.Running, "status");
    }

    public static void CappedBalanceRequestRejectsMovesFromFailed()
    {
        var item = CappedBalanceRequest.NewCappedBalanceRequest("code-716", "title-655");
        item.Status = CappedBalanceRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedBalanceRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedBalanceRequestStartsInNew();
        CappedBalanceRequestMovesToRunning();
        CappedBalanceRequestRejectsMovesFromFailed();
    }
}
