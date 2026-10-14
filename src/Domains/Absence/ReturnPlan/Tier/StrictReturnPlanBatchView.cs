namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict return plan batch.</summary>
public sealed record StrictReturnPlanBatchView(string CodeText, string Title, string BatchSizeText, string OwnerId, string Pinned, string MaximumText, string Checksum, string Summary, string StatusLabel)
{
    public static StrictReturnPlanBatchView ToStrictReturnPlanBatchView(StrictReturnPlanBatch item)
    {
        return new StrictReturnPlanBatchView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            $"{item.Pinned}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 40),
            Text.Truncate(item.Summary, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictReturnPlanBatchView> ToStrictReturnPlanBatchViews(List<StrictReturnPlanBatch> items)
    {
        var views = new List<StrictReturnPlanBatchView>();
        foreach (var item in items)
        {
            views.Add(ToStrictReturnPlanBatchView(item));
        }
        return views;
    }

    public static double SumStrictReturnPlanBatchView(StrictReturnPlanBatchView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedMaximum;
    }
}
