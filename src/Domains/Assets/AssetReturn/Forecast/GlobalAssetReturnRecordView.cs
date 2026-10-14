namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global asset return record.</summary>
public sealed record GlobalAssetReturnRecordView(string Code, string Name, string StartsAt, string Archived, string ExpiresAt, string ChecksumText, string Segment, string Capacity, string Pinned, string JobCode, string StatusLabel)
{
    public static GlobalAssetReturnRecordView ToGlobalAssetReturnRecordView(GlobalAssetReturnRecord item)
    {
        return new GlobalAssetReturnRecordView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Segment,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAssetReturnRecordView> ToGlobalAssetReturnRecordViews(List<GlobalAssetReturnRecord> items)
    {
        var views = new List<GlobalAssetReturnRecordView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAssetReturnRecordView(item));
        }
        return views;
    }

    public static double SumGlobalAssetReturnRecordView(GlobalAssetReturnRecordView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedExpiresAt + (double)parsedCapacity;
    }
}
