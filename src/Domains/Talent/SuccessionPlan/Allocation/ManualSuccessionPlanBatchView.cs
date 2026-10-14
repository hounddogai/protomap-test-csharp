namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual succession plan batch.</summary>
public sealed record ManualSuccessionPlanBatchView(string Code, string Title, string StartsAtText, string OwnerIdText, string Label, string Name, string StatusLabel)
{
    public static ManualSuccessionPlanBatchView ToManualSuccessionPlanBatchView(ManualSuccessionPlanBatch item)
    {
        return new ManualSuccessionPlanBatchView(
            item.Code,
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            Text.Truncate(item.Label, 120),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualSuccessionPlanBatchView> ToManualSuccessionPlanBatchViews(List<ManualSuccessionPlanBatch> items)
    {
        var views = new List<ManualSuccessionPlanBatchView>();
        foreach (var item in items)
        {
            views.Add(ToManualSuccessionPlanBatchView(item));
        }
        return views;
    }

    public static double SumManualSuccessionPlanBatchView(ManualSuccessionPlanBatchView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
