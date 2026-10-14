namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using System.Globalization;

/// <summary>How screens and exports show a pending policy adjustment.</summary>
public sealed record PendingPolicyAdjustmentView(string Code, string Title, string ExternalRef, string Location, string BatchSize, string StatusLabel)
{
    public static PendingPolicyAdjustmentView ToPendingPolicyAdjustmentView(PendingPolicyAdjustment item)
    {
        return new PendingPolicyAdjustmentView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Location,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPolicyAdjustmentView> ToPendingPolicyAdjustmentViews(List<PendingPolicyAdjustment> items)
    {
        var views = new List<PendingPolicyAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPolicyAdjustmentView(item));
        }
        return views;
    }

    public static double SumPendingPolicyAdjustmentView(PendingPolicyAdjustmentView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
