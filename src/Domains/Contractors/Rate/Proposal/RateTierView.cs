namespace Acme.Workforce.Contractors.Rate.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a rate tier.</summary>
public sealed record RateTierView(string CodeText, string Name, string Ceiling, string JobCodeText, string MarginText, string BadgeText, string StatusLabel)
{
    public static RateTierView ToRateTierView(RateTier item)
    {
        return new RateTierView(
            item.Code,
            item.Name,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateTierView> ToRateTierViews(List<RateTier> items)
    {
        var views = new List<RateTierView>();
        foreach (var item in items)
        {
            views.Add(ToRateTierView(item));
        }
        return views;
    }

    public static double SumRateTierView(RateTierView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedMargin;
    }
}
