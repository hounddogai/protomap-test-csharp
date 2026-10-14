namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopWindowViewChecks
{
    public static void LaptopWindowViewShowsTheStatusInCapitals()
    {
        var item = LaptopWindow.NewLaptopWindow("code-917", "name-418");
        var view = LaptopWindowView.ToLaptopWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopWindowView.ToLaptopWindowViews(new List<LaptopWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopWindowViewShowsTheStatusInCapitals();
    }
}
