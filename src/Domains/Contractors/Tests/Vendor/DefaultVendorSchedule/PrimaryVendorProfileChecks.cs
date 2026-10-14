namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class PrimaryVendorProfileChecks
{
    public static void PrimaryVendorProfileStartsInCreated()
    {
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("code-259", "title-602");
        Check.Equal(item.Status, PrimaryVendorProfileStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryVendorProfileMovesToPublished()
    {
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("code-259", "title-602");
        try
        {
            item.MoveTo(PrimaryVendorProfileStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryVendorProfileStatus.Published, "status");
    }

    public static void PrimaryVendorProfileRejectsMovesFromRetired()
    {
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("code-259", "title-602");
        item.Status = PrimaryVendorProfileStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryVendorProfileStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryVendorProfileStartsInCreated();
        PrimaryVendorProfileMovesToPublished();
        PrimaryVendorProfileRejectsMovesFromRetired();
    }
}
