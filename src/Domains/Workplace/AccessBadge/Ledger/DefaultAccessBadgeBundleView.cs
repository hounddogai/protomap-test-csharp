namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default access badge bundle.</summary>
public sealed record DefaultAccessBadgeBundleView(string Code, string Name, string AttemptsText, string Capacity, string MinimumText, string Weight, string Priority, string VisibleText, string Locale, string Rate, string StatusLabel)
{
    public static DefaultAccessBadgeBundleView ToDefaultAccessBadgeBundleView(DefaultAccessBadgeBundle item)
    {
        return new DefaultAccessBadgeBundleView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Locale,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAccessBadgeBundleView> ToDefaultAccessBadgeBundleViews(List<DefaultAccessBadgeBundle> items)
    {
        var views = new List<DefaultAccessBadgeBundleView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAccessBadgeBundleView(item));
        }
        return views;
    }

    public static double SumDefaultAccessBadgeBundleView(DefaultAccessBadgeBundleView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedCapacity + parsedMinimum;
    }
}
