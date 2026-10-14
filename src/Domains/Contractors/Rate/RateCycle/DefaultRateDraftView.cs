namespace Acme.Workforce.Contractors.Rate.RateCycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default rate draft.</summary>
public sealed record DefaultRateDraftView(string Code, string Name, string Summary, string DurationMinutesText, string CapacityText, string StatusLabel)
{
    public static DefaultRateDraftView ToDefaultRateDraftView(DefaultRateDraft item)
    {
        return new DefaultRateDraftView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Summary,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultRateDraftView> ToDefaultRateDraftViews(List<DefaultRateDraft> items)
    {
        var views = new List<DefaultRateDraftView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultRateDraftView(item));
        }
        return views;
    }

    public static double SumDefaultRateDraftView(DefaultRateDraftView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedCapacity;
    }
}
