namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount metric.</summary>
public sealed record HeadcountMetricView(string Code, string Name, string Attempts, string LocationText, string Visible, string NotesText, string ActiveText, string EndsAt, string Channel, string StartsAtText, string StatusLabel)
{
    public static HeadcountMetricView ToHeadcountMetricView(HeadcountMetric item)
    {
        return new HeadcountMetricView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Visible}",
            item.Notes,
            $"{item.Active}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountMetricView> ToHeadcountMetricViews(List<HeadcountMetric> items)
    {
        var views = new List<HeadcountMetricView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountMetricView(item));
        }
        return views;
    }

    public static double SumHeadcountMetricView(HeadcountMetricView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedEndsAt + (double)parsedStartsAt;
    }
}
