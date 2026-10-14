namespace Acme.Workforce.Assets.AssetReturn.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled asset return batch.</summary>
public sealed record ScheduledAssetReturnBatchView(string Code, string Name, string Budget, string Grade, string Notes, string DurationMinutesText, string ContactEmailText, string Version, string StatusLabel)
{
    public static ScheduledAssetReturnBatchView ToScheduledAssetReturnBatchView(ScheduledAssetReturnBatch item)
    {
        return new ScheduledAssetReturnBatchView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            Text.Truncate(item.Notes, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAssetReturnBatchView> ToScheduledAssetReturnBatchViews(List<ScheduledAssetReturnBatch> items)
    {
        var views = new List<ScheduledAssetReturnBatchView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAssetReturnBatchView(item));
        }
        return views;
    }

    public static double SumScheduledAssetReturnBatchView(ScheduledAssetReturnBatchView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedDurationMinutes + (double)parsedVersion;
    }
}
