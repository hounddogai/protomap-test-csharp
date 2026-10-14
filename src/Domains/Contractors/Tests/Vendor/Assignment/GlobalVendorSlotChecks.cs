namespace Acme.Workforce.Contractors.Tests.Vendor.Assignment;

using Acme.Workforce.Contractors.Vendor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalVendorSlotChecks
{
    public static void GlobalVendorSlotStartsInIdle()
    {
        var item = GlobalVendorSlot.NewGlobalVendorSlot("code-819", "label-87");
        Check.Equal(item.Status, GlobalVendorSlotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalVendorSlotMovesToWarming()
    {
        var item = GlobalVendorSlot.NewGlobalVendorSlot("code-819", "label-87");
        try
        {
            item.MoveTo(GlobalVendorSlotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalVendorSlotStatus.Warming, "status");
    }

    public static void GlobalVendorSlotRejectsMovesFromStopped()
    {
        var item = GlobalVendorSlot.NewGlobalVendorSlot("code-819", "label-87");
        item.Status = GlobalVendorSlotStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalVendorSlotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalVendorSlotStartsInIdle();
        GlobalVendorSlotMovesToWarming();
        GlobalVendorSlotRejectsMovesFromStopped();
    }
}
