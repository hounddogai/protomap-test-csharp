namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class GarnishmentSlotChecks
{
    public static void GarnishmentSlotStartsInOpen()
    {
        var item = GarnishmentSlot.NewGarnishmentSlot("code-693", "title-683");
        Check.Equal(item.Status, GarnishmentSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentSlotMovesToReopened()
    {
        var item = GarnishmentSlot.NewGarnishmentSlot("code-693", "title-683");
        try
        {
            item.MoveTo(GarnishmentSlotStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentSlotStatus.Reopened, "status");
    }

    public static void GarnishmentSlotRejectsMovesFromArchived()
    {
        var item = GarnishmentSlot.NewGarnishmentSlot("code-693", "title-683");
        item.Status = GarnishmentSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(GarnishmentSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        GarnishmentSlotStartsInOpen();
        GarnishmentSlotMovesToReopened();
        GarnishmentSlotRejectsMovesFromArchived();
    }
}
