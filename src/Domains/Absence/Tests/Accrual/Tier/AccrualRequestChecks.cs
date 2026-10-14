namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualRequestChecks
{
    public static void AccrualRequestStartsInOpen()
    {
        var item = AccrualRequest.NewAccrualRequest("code-753", "name-408");
        Check.Equal(item.Status, AccrualRequestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualRequestMovesToInReview()
    {
        var item = AccrualRequest.NewAccrualRequest("code-753", "name-408");
        try
        {
            item.MoveTo(AccrualRequestStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualRequestStatus.InReview, "status");
    }

    public static void AccrualRequestRejectsMovesFromArchived()
    {
        var item = AccrualRequest.NewAccrualRequest("code-753", "name-408");
        item.Status = AccrualRequestStatus.Archived;
        Check.Fails(() => item.MoveTo(AccrualRequestStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccrualRequestStartsInOpen();
        AccrualRequestMovesToInReview();
        AccrualRequestRejectsMovesFromArchived();
    }
}
