namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentMappingChecks
{
    public static void GarnishmentMappingStartsInPlanned()
    {
        var item = GarnishmentMapping.NewGarnishmentMapping("code-408", "name-415");
        Check.Equal(item.Status, GarnishmentMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentMappingMovesToCancelled()
    {
        var item = GarnishmentMapping.NewGarnishmentMapping("code-408", "name-415");
        try
        {
            item.MoveTo(GarnishmentMappingStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentMappingStatus.Cancelled, "status");
    }

    public static void GarnishmentMappingRejectsMovesFromCancelled()
    {
        var item = GarnishmentMapping.NewGarnishmentMapping("code-408", "name-415");
        item.Status = GarnishmentMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GarnishmentMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GarnishmentMappingStartsInPlanned();
        GarnishmentMappingMovesToCancelled();
        GarnishmentMappingRejectsMovesFromCancelled();
    }
}
