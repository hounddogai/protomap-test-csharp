namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LenientLaptopWindowViewChecks
{
    public static void LenientLaptopWindowViewShowsTheStatusInCapitals()
    {
        var item = LenientLaptopWindow.NewLenientLaptopWindow("code-360", "name-368");
        var view = LenientLaptopWindowView.ToLenientLaptopWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientLaptopWindowView.ToLenientLaptopWindowViews(new List<LenientLaptopWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientLaptopWindowViewShowsTheStatusInCapitals();
    }
}
