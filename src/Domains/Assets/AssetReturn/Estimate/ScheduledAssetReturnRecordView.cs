namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a scheduled asset return record.</summary>
public sealed record ScheduledAssetReturnRecordView(string CodeText, string Label, string Version, string JobCode, string TimezoneText, string Ratio, string LocationText, string StatusLabel)
{
    public static ScheduledAssetReturnRecordView ToScheduledAssetReturnRecordView(ScheduledAssetReturnRecord item)
    {
        return new ScheduledAssetReturnRecordView(
            item.Code,
            item.Label,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Timezone,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAssetReturnRecordView> ToScheduledAssetReturnRecordViews(List<ScheduledAssetReturnRecord> items)
    {
        var views = new List<ScheduledAssetReturnRecordView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAssetReturnRecordView(item));
        }
        return views;
    }

    public static double SumScheduledAssetReturnRecordView(ScheduledAssetReturnRecordView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedRatio;
    }
}
