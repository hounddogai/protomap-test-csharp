namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a scheduled pay run cycle.</summary>
public sealed record ScheduledPayRunCycleView(string CodeText, string TitleText, string RatioText, string CreatedAt, string Pinned, string Phone, string Version, string ManagerId, string Team, string StatusLabel)
{
    public static ScheduledPayRunCycleView ToScheduledPayRunCycleView(ScheduledPayRunCycle item)
    {
        return new ScheduledPayRunCycleView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Phone,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPayRunCycleView> ToScheduledPayRunCycleViews(List<ScheduledPayRunCycle> items)
    {
        var views = new List<ScheduledPayRunCycleView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPayRunCycleView(item));
        }
        return views;
    }

    public static double SumScheduledPayRunCycleView(ScheduledPayRunCycleView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedCreatedAt + (double)parsedVersion;
    }
}
