namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingLaptopTargetViewChecks
{
    public static void PendingLaptopTargetViewShowsTheStatusInCapitals()
    {
        var item = PendingLaptopTarget.NewPendingLaptopTarget("code-118", "label-24");
        var view = PendingLaptopTargetView.ToPendingLaptopTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingLaptopTargetView.ToPendingLaptopTargetViews(new List<PendingLaptopTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingLaptopTargetViewShowsTheStatusInCapitals();
    }
}
