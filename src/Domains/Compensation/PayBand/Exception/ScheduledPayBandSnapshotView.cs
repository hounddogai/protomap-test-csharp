namespace Acme.Workforce.Compensation.PayBand.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled pay band snapshot.</summary>
public sealed record ScheduledPayBandSnapshotView(string Code, string NameText, string CreatedAtText, string CostCenter, string MarginText, string WindowDays, string Rating, string CeilingText, string Retries, string JobCode, string StatusLabel)
{
    public static ScheduledPayBandSnapshotView ToScheduledPayBandSnapshotView(ScheduledPayBandSnapshot item)
    {
        return new ScheduledPayBandSnapshotView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPayBandSnapshotView> ToScheduledPayBandSnapshotViews(List<ScheduledPayBandSnapshot> items)
    {
        var views = new List<ScheduledPayBandSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPayBandSnapshotView(item));
        }
        return views;
    }

    public static double SumScheduledPayBandSnapshotView(ScheduledPayBandSnapshotView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedMargin + (double)parsedWindowDays;
    }
}
