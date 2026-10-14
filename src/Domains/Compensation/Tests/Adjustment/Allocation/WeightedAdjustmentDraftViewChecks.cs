namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class WeightedAdjustmentDraftViewChecks
{
    public static void WeightedAdjustmentDraftViewShowsTheStatusInCapitals()
    {
        var item = WeightedAdjustmentDraft.NewWeightedAdjustmentDraft("code-912", "label-478");
        var view = WeightedAdjustmentDraftView.ToWeightedAdjustmentDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedAdjustmentDraftView.ToWeightedAdjustmentDraftViews(new List<WeightedAdjustmentDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedAdjustmentDraftViewShowsTheStatusInCapitals();
    }
}
