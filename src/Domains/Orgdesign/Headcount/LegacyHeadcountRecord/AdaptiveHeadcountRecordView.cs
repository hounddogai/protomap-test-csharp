namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive headcount record.</summary>
public sealed record AdaptiveHeadcountRecordView(string CodeText, string Label, string BatchSize, string FloorValue, string JobCode, string Region, string UpdatedAt, string Approved, string Sequence, string Margin, string Attempts, string StatusLabel)
{
    public static AdaptiveHeadcountRecordView ToAdaptiveHeadcountRecordView(AdaptiveHeadcountRecord item)
    {
        return new AdaptiveHeadcountRecordView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            Text.Truncate(item.Region, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveHeadcountRecordView> ToAdaptiveHeadcountRecordViews(List<AdaptiveHeadcountRecord> items)
    {
        var views = new List<AdaptiveHeadcountRecordView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveHeadcountRecordView(item));
        }
        return views;
    }

    public static double SumAdaptiveHeadcountRecordView(AdaptiveHeadcountRecordView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedFloorValue + (double)parsedUpdatedAt;
    }
}
