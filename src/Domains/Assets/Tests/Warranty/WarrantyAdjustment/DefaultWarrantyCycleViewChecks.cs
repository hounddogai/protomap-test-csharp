namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class DefaultWarrantyCycleViewChecks
{
    public static void DefaultWarrantyCycleViewShowsTheStatusInCapitals()
    {
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("code-226", "name-125");
        var view = DefaultWarrantyCycleView.ToDefaultWarrantyCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultWarrantyCycleView.ToDefaultWarrantyCycleViews(new List<DefaultWarrantyCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultWarrantyCycleViewShowsTheStatusInCapitals();
    }
}
