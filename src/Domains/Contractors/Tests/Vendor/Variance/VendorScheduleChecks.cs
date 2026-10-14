namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorScheduleChecks
{
    public static void VendorScheduleStartsInOpen()
    {
        var item = VendorSchedule.NewVendorSchedule("code-94", "name-598");
        Check.Equal(item.Status, VendorScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorScheduleMovesToResolved()
    {
        var item = VendorSchedule.NewVendorSchedule("code-94", "name-598");
        try
        {
            item.MoveTo(VendorScheduleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorScheduleStatus.Resolved, "status");
    }

    public static void VendorScheduleRejectsMovesFromArchived()
    {
        var item = VendorSchedule.NewVendorSchedule("code-94", "name-598");
        item.Status = VendorScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(VendorScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        VendorScheduleStartsInOpen();
        VendorScheduleMovesToResolved();
        VendorScheduleRejectsMovesFromArchived();
    }
}
