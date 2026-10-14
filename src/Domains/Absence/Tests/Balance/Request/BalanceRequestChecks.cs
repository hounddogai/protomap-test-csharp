namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BalanceRequestChecks
{
    public static void BalanceRequestStartsInCreated()
    {
        var item = BalanceRequest.NewBalanceRequest("code-970", "name-361");
        Check.Equal(item.Status, BalanceRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceRequestMovesToPublished()
    {
        var item = BalanceRequest.NewBalanceRequest("code-970", "name-361");
        try
        {
            item.MoveTo(BalanceRequestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceRequestStatus.Published, "status");
    }

    public static void BalanceRequestRejectsMovesFromRetired()
    {
        var item = BalanceRequest.NewBalanceRequest("code-970", "name-361");
        item.Status = BalanceRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(BalanceRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        BalanceRequestStartsInCreated();
        BalanceRequestMovesToPublished();
        BalanceRequestRejectsMovesFromRetired();
    }
}
