namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskAdjustmentChecks
{
    public static void DeskAdjustmentStartsInCreated()
    {
        var item = DeskAdjustment.NewDeskAdjustment("code-467", "title-345");
        Check.Equal(item.Status, DeskAdjustmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskAdjustmentMovesToPublished()
    {
        var item = DeskAdjustment.NewDeskAdjustment("code-467", "title-345");
        try
        {
            item.MoveTo(DeskAdjustmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskAdjustmentStatus.Published, "status");
    }

    public static void DeskAdjustmentRejectsMovesFromRetired()
    {
        var item = DeskAdjustment.NewDeskAdjustment("code-467", "title-345");
        item.Status = DeskAdjustmentStatus.Retired;
        Check.Fails(() => item.MoveTo(DeskAdjustmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeskAdjustmentStartsInCreated();
        DeskAdjustmentMovesToPublished();
        DeskAdjustmentRejectsMovesFromRetired();
    }
}
