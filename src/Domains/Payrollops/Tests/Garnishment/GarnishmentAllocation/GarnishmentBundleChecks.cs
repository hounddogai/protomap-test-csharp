namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class GarnishmentBundleChecks
{
    public static void GarnishmentBundleStartsInNew()
    {
        var item = GarnishmentBundle.NewGarnishmentBundle("code-123", "title-782");
        Check.Equal(item.Status, GarnishmentBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentBundleMovesToRunning()
    {
        var item = GarnishmentBundle.NewGarnishmentBundle("code-123", "title-782");
        try
        {
            item.MoveTo(GarnishmentBundleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentBundleStatus.Running, "status");
    }

    public static void GarnishmentBundleRejectsMovesFromFailed()
    {
        var item = GarnishmentBundle.NewGarnishmentBundle("code-123", "title-782");
        item.Status = GarnishmentBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(GarnishmentBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        GarnishmentBundleStartsInNew();
        GarnishmentBundleMovesToRunning();
        GarnishmentBundleRejectsMovesFromFailed();
    }
}
