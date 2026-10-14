namespace Acme.Workforce.Assets.AssetReturn.Summary;

using System.Globalization;

/// <summary>How screens and exports show a asset return schedule.</summary>
public sealed record AssetReturnScheduleView(string CodeText, string Title, string TenureMonths, string EmployeeIdText, string RevisionText, string StatusLabel)
{
    public static AssetReturnScheduleView ToAssetReturnScheduleView(AssetReturnSchedule item)
    {
        return new AssetReturnScheduleView(
            item.Code,
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnScheduleView> ToAssetReturnScheduleViews(List<AssetReturnSchedule> items)
    {
        var views = new List<AssetReturnScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnScheduleView(item));
        }
        return views;
    }

    public static double SumAssetReturnScheduleView(AssetReturnScheduleView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedRevision;
    }
}
