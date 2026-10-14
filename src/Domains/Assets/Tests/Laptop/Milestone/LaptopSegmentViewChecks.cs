namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopSegmentViewChecks
{
    public static void LaptopSegmentViewShowsTheStatusInCapitals()
    {
        var item = LaptopSegment.NewLaptopSegment("code-342", "name-13");
        var view = LaptopSegmentView.ToLaptopSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopSegmentView.ToLaptopSegmentViews(new List<LaptopSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopSegmentViewShowsTheStatusInCapitals();
    }
}
