namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant window.</summary>
public sealed record EquityGrantWindowView(string Code, string Label, string Visible, string Minimum, string Limit, string Score, string StatusLabel)
{
    public static EquityGrantWindowView ToEquityGrantWindowView(EquityGrantWindow item)
    {
        return new EquityGrantWindowView(
            Text.Truncate(item.Code, 80),
            item.Label,
            $"{item.Visible}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantWindowView> ToEquityGrantWindowViews(List<EquityGrantWindow> items)
    {
        var views = new List<EquityGrantWindowView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantWindowView(item));
        }
        return views;
    }

    public static double SumEquityGrantWindowView(EquityGrantWindowView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedLimit + parsedScore;
    }
}
