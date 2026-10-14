namespace Acme.Workforce.Absence.Balance.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance forecast.</summary>
public sealed record BalanceForecastView(string Code, string Name, string Region, string VersionText, string Phone, string NotesText, string OwnerId, string GradeText, string MarginText, string SegmentText, string Ratio, string StatusLabel)
{
    public static BalanceForecastView ToBalanceForecastView(BalanceForecast item)
    {
        return new BalanceForecastView(
            item.Code,
            item.Name,
            item.Region,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            Text.Truncate(item.Notes, 64),
            item.OwnerId,
            item.Grade,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceForecastView> ToBalanceForecastViews(List<BalanceForecast> items)
    {
        var views = new List<BalanceForecastView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceForecastView(item));
        }
        return views;
    }

    public static double SumBalanceForecastView(BalanceForecastView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMargin + parsedRatio;
    }
}
