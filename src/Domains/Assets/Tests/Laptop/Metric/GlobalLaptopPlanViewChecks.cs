namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class GlobalLaptopPlanViewChecks
{
    public static void GlobalLaptopPlanViewShowsTheStatusInCapitals()
    {
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("code-804", "label-32");
        var view = GlobalLaptopPlanView.ToGlobalLaptopPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalLaptopPlanView.ToGlobalLaptopPlanViews(new List<GlobalLaptopPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalLaptopPlanViewShowsTheStatusInCapitals();
    }
}
