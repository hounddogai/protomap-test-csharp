namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LicenseSeatAllocationViewChecks
{
    public static void LicenseSeatAllocationViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("code-550", "label-156");
        var view = LicenseSeatAllocationView.ToLicenseSeatAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatAllocationView.ToLicenseSeatAllocationViews(new List<LicenseSeatAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatAllocationViewShowsTheStatusInCapitals();
    }
}
