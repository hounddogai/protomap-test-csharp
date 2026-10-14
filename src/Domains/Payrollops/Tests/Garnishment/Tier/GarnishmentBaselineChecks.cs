namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentBaselineChecks
{
    public static void GarnishmentBaselineStartsInCreated()
    {
        var item = GarnishmentBaseline.NewGarnishmentBaseline("code-836", "label-332");
        Check.Equal(item.Status, GarnishmentBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentBaselineMovesToPublished()
    {
        var item = GarnishmentBaseline.NewGarnishmentBaseline("code-836", "label-332");
        try
        {
            item.MoveTo(GarnishmentBaselineStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentBaselineStatus.Published, "status");
    }

    public static void GarnishmentBaselineRejectsMovesFromRetired()
    {
        var item = GarnishmentBaseline.NewGarnishmentBaseline("code-836", "label-332");
        item.Status = GarnishmentBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(GarnishmentBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        GarnishmentBaselineStartsInCreated();
        GarnishmentBaselineMovesToPublished();
        GarnishmentBaselineRejectsMovesFromRetired();
    }
}
