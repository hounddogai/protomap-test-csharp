namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle summary.</summary>
public sealed record MeritCycleSummaryView(string Code, string Label, string BadgeText, string Notes, string Margin, string Weight, string Location, string StatusLabel)
{
    public static MeritCycleSummaryView ToMeritCycleSummaryView(MeritCycleSummary item)
    {
        return new MeritCycleSummaryView(
            item.Code,
            item.Label,
            item.Badge,
            item.Notes,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleSummaryView> ToMeritCycleSummaryViews(List<MeritCycleSummary> items)
    {
        var views = new List<MeritCycleSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleSummaryView(item));
        }
        return views;
    }

    public static double SumMeritCycleSummaryView(MeritCycleSummaryView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedMargin + parsedWeight;
    }
}
