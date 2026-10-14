namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RollingLaptopMappingViewChecks
{
    public static void RollingLaptopMappingViewShowsTheStatusInCapitals()
    {
        var item = RollingLaptopMapping.NewRollingLaptopMapping("code-425", "name-878");
        var view = RollingLaptopMappingView.ToRollingLaptopMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingLaptopMappingView.ToRollingLaptopMappingViews(new List<RollingLaptopMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingLaptopMappingViewShowsTheStatusInCapitals();
    }
}
