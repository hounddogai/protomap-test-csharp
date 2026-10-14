namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveLaptopCheckpointViewChecks
{
    public static void AdaptiveLaptopCheckpointViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("code-968", "label-883");
        var view = AdaptiveLaptopCheckpointView.ToAdaptiveLaptopCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveLaptopCheckpointView.ToAdaptiveLaptopCheckpointViews(new List<AdaptiveLaptopCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveLaptopCheckpointViewShowsTheStatusInCapitals();
    }
}
