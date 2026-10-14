namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMilestoneViewChecks
{
    public static void AdaptiveWarrantyMilestoneViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("code-140", "label-164");
        var view = AdaptiveWarrantyMilestoneView.ToAdaptiveWarrantyMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveWarrantyMilestoneView.ToAdaptiveWarrantyMilestoneViews(new List<AdaptiveWarrantyMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMilestoneViewShowsTheStatusInCapitals();
    }
}
