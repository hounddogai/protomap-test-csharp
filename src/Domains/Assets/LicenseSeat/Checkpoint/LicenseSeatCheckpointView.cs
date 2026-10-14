namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat checkpoint.</summary>
public sealed record LicenseSeatCheckpointView(string Code, string Title, string Department, string Category, string Locale, string Threshold, string JobCode, string Budget, string StartsAtText, string EndsAt, string StatusLabel)
{
    public static LicenseSeatCheckpointView ToLicenseSeatCheckpointView(LicenseSeatCheckpoint item)
    {
        return new LicenseSeatCheckpointView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Department,
            item.Category,
            Text.Truncate(item.Locale, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatCheckpointView> ToLicenseSeatCheckpointViews(List<LicenseSeatCheckpoint> items)
    {
        var views = new List<LicenseSeatCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatCheckpointView(item));
        }
        return views;
    }

    public static double SumLicenseSeatCheckpointView(LicenseSeatCheckpointView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedBudget + (double)parsedStartsAt;
    }
}
