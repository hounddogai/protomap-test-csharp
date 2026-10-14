namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GarnishmentQuotaChecks
{
    public static void GarnishmentQuotaStartsInProposed()
    {
        var item = GarnishmentQuota.NewGarnishmentQuota("code-885", "name-500");
        Check.Equal(item.Status, GarnishmentQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentQuotaMovesToApproved()
    {
        var item = GarnishmentQuota.NewGarnishmentQuota("code-885", "name-500");
        try
        {
            item.MoveTo(GarnishmentQuotaStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentQuotaStatus.Approved, "status");
    }

    public static void GarnishmentQuotaRejectsMovesFromWithdrawn()
    {
        var item = GarnishmentQuota.NewGarnishmentQuota("code-885", "name-500");
        item.Status = GarnishmentQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GarnishmentQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GarnishmentQuotaStartsInProposed();
        GarnishmentQuotaMovesToApproved();
        GarnishmentQuotaRejectsMovesFromWithdrawn();
    }
}
