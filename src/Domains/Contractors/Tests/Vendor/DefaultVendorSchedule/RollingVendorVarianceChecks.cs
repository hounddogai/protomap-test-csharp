namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class RollingVendorVarianceChecks
{
    public static void RollingVendorVarianceStartsInDraft()
    {
        var item = RollingVendorVariance.NewRollingVendorVariance("code-223", "name-331");
        Check.Equal(item.Status, RollingVendorVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingVendorVarianceMovesToActive()
    {
        var item = RollingVendorVariance.NewRollingVendorVariance("code-223", "name-331");
        try
        {
            item.MoveTo(RollingVendorVarianceStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingVendorVarianceStatus.Active, "status");
    }

    public static void RollingVendorVarianceRejectsMovesFromClosed()
    {
        var item = RollingVendorVariance.NewRollingVendorVariance("code-223", "name-331");
        item.Status = RollingVendorVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingVendorVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingVendorVarianceStartsInDraft();
        RollingVendorVarianceMovesToActive();
        RollingVendorVarianceRejectsMovesFromClosed();
    }
}
