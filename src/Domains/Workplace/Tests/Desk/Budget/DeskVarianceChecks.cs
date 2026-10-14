namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeskVarianceChecks
{
    public static void DeskVarianceStartsInPlanned()
    {
        var item = DeskVariance.NewDeskVariance("code-207", "name-369");
        Check.Equal(item.Status, DeskVarianceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskVarianceMovesToInProgress()
    {
        var item = DeskVariance.NewDeskVariance("code-207", "name-369");
        try
        {
            item.MoveTo(DeskVarianceStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskVarianceStatus.InProgress, "status");
    }

    public static void DeskVarianceRejectsMovesFromCancelled()
    {
        var item = DeskVariance.NewDeskVariance("code-207", "name-369");
        item.Status = DeskVarianceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DeskVarianceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DeskVarianceStartsInPlanned();
        DeskVarianceMovesToInProgress();
        DeskVarianceRejectsMovesFromCancelled();
    }
}
