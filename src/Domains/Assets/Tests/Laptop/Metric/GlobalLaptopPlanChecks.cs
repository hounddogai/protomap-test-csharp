namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class GlobalLaptopPlanChecks
{
    public static void GlobalLaptopPlanStartsInOpen()
    {
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("code-804", "label-32");
        Check.Equal(item.Status, GlobalLaptopPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalLaptopPlanMovesToInReview()
    {
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("code-804", "label-32");
        try
        {
            item.MoveTo(GlobalLaptopPlanStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalLaptopPlanStatus.InReview, "status");
    }

    public static void GlobalLaptopPlanRejectsMovesFromArchived()
    {
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("code-804", "label-32");
        item.Status = GlobalLaptopPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalLaptopPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalLaptopPlanStartsInOpen();
        GlobalLaptopPlanMovesToInReview();
        GlobalLaptopPlanRejectsMovesFromArchived();
    }
}
