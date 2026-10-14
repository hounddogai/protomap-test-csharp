namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan entry.</summary>
public sealed record SuccessionPlanEntryView(string CodeText, string Title, string Hours, string TimezoneText, string Rating, string Margin, string CreatedAtText, string StatusLabel)
{
    public static SuccessionPlanEntryView ToSuccessionPlanEntryView(SuccessionPlanEntry item)
    {
        return new SuccessionPlanEntryView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 120),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanEntryView> ToSuccessionPlanEntryViews(List<SuccessionPlanEntry> items)
    {
        var views = new List<SuccessionPlanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanEntryView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanEntryView(SuccessionPlanEntryView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedHours + parsedRating + parsedMargin;
    }
}
