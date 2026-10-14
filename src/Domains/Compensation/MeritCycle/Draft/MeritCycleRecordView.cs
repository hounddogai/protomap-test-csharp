namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle record.</summary>
public sealed record MeritCycleRecordView(string Code, string NameText, string Margin, string CostCenter, string Channel, string Locale, string StartsAt, string Threshold, string Description, string SalaryBand, string ExpiresAtText, string StatusLabel)
{
    public static MeritCycleRecordView ToMeritCycleRecordView(MeritCycleRecord item)
    {
        return new MeritCycleRecordView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 64),
            item.Channel,
            Text.Truncate(item.Locale, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            item.SalaryBand,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleRecordView> ToMeritCycleRecordViews(List<MeritCycleRecord> items)
    {
        var views = new List<MeritCycleRecordView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleRecordView(item));
        }
        return views;
    }

    public static double SumMeritCycleRecordView(MeritCycleRecordView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedStartsAt + parsedThreshold;
    }
}
