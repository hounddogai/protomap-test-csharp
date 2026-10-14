namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class RollingVendorQuotaChecks
{
    public static void RollingVendorQuotaStartsInPlanned()
    {
        var item = RollingVendorQuota.NewRollingVendorQuota("code-268", "label-803");
        Check.Equal(item.Status, RollingVendorQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingVendorQuotaMovesToScheduled()
    {
        var item = RollingVendorQuota.NewRollingVendorQuota("code-268", "label-803");
        try
        {
            item.MoveTo(RollingVendorQuotaStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingVendorQuotaStatus.Scheduled, "status");
    }

    public static void RollingVendorQuotaRejectsMovesFromCancelled()
    {
        var item = RollingVendorQuota.NewRollingVendorQuota("code-268", "label-803");
        item.Status = RollingVendorQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingVendorQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingVendorQuotaStartsInPlanned();
        RollingVendorQuotaMovesToScheduled();
        RollingVendorQuotaRejectsMovesFromCancelled();
    }
}
