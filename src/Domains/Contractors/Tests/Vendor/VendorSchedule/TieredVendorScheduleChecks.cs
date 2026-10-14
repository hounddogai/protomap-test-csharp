namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class TieredVendorScheduleChecks
{
    public static void TieredVendorScheduleStartsInPlanned()
    {
        var item = TieredVendorSchedule.NewTieredVendorSchedule("code-369", "label-132");
        Check.Equal(item.Status, TieredVendorScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredVendorScheduleMovesToCancelled()
    {
        var item = TieredVendorSchedule.NewTieredVendorSchedule("code-369", "label-132");
        try
        {
            item.MoveTo(TieredVendorScheduleStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredVendorScheduleStatus.Cancelled, "status");
    }

    public static void TieredVendorScheduleRejectsMovesFromCancelled()
    {
        var item = TieredVendorSchedule.NewTieredVendorSchedule("code-369", "label-132");
        item.Status = TieredVendorScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredVendorScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredVendorScheduleStartsInPlanned();
        TieredVendorScheduleMovesToCancelled();
        TieredVendorScheduleRejectsMovesFromCancelled();
    }
}
