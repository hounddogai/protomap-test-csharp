namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopMilestoneViewChecks
{
    public static void LaptopMilestoneViewShowsTheStatusInCapitals()
    {
        var item = LaptopMilestone.NewLaptopMilestone("code-794", "title-76");
        var view = LaptopMilestoneView.ToLaptopMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopMilestoneView.ToLaptopMilestoneViews(new List<LaptopMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopMilestoneViewShowsTheStatusInCapitals();
    }
}
