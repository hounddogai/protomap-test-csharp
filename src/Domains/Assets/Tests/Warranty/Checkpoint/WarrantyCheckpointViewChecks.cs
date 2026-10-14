namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyCheckpointViewChecks
{
    public static void WarrantyCheckpointViewShowsTheStatusInCapitals()
    {
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("code-223", "title-507");
        var view = WarrantyCheckpointView.ToWarrantyCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyCheckpointView.ToWarrantyCheckpointViews(new List<WarrantyCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyCheckpointViewShowsTheStatusInCapitals();
    }
}
