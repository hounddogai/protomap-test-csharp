namespace Acme.Workforce.Compensation.MeritCycle.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle quota.</summary>
public sealed record MeritCycleQuotaView(string Code, string Title, string Score, string Category, string Approved, string Currency, string Location, string FloorValue, string Notes, string SummaryText, string UpdatedAt, string StatusLabel)
{
    public static MeritCycleQuotaView ToMeritCycleQuotaView(MeritCycleQuota item)
    {
        return new MeritCycleQuotaView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Approved}",
            Text.Truncate(item.Currency, 64),
            Text.Truncate(item.Location, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Summary,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleQuotaView> ToMeritCycleQuotaViews(List<MeritCycleQuota> items)
    {
        var views = new List<MeritCycleQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleQuotaView(item));
        }
        return views;
    }

    public static double SumMeritCycleQuotaView(MeritCycleQuotaView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedScore + parsedFloorValue + (double)parsedUpdatedAt;
    }
}
