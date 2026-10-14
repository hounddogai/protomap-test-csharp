namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRollupChecks
{
    public static void GarnishmentRollupStartsInOpen()
    {
        var item = GarnishmentRollup.NewGarnishmentRollup("code-256", "label-702");
        Check.Equal(item.Status, GarnishmentRollupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentRollupMovesToReopened()
    {
        var item = GarnishmentRollup.NewGarnishmentRollup("code-256", "label-702");
        try
        {
            item.MoveTo(GarnishmentRollupStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentRollupStatus.Reopened, "status");
    }

    public static void GarnishmentRollupRejectsMovesFromArchived()
    {
        var item = GarnishmentRollup.NewGarnishmentRollup("code-256", "label-702");
        item.Status = GarnishmentRollupStatus.Archived;
        Check.Fails(() => item.MoveTo(GarnishmentRollupStatus.Open), "move");
    }

    public static void RunAll()
    {
        GarnishmentRollupStartsInOpen();
        GarnishmentRollupMovesToReopened();
        GarnishmentRollupRejectsMovesFromArchived();
    }
}
