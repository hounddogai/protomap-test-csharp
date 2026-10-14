namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopBundleViewChecks
{
    public static void LaptopBundleViewShowsTheStatusInCapitals()
    {
        var item = LaptopBundle.NewLaptopBundle("code-510", "title-454");
        var view = LaptopBundleView.ToLaptopBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopBundleView.ToLaptopBundleViews(new List<LaptopBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopBundleViewShowsTheStatusInCapitals();
    }
}
