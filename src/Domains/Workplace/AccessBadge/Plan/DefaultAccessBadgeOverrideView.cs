namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default access badge override.</summary>
public sealed record DefaultAccessBadgeOverrideView(string Code, string TitleText, string Badge, string JobCode, string Discount, string Minimum, string StatusLabel)
{
    public static DefaultAccessBadgeOverrideView ToDefaultAccessBadgeOverrideView(DefaultAccessBadgeOverride item)
    {
        return new DefaultAccessBadgeOverrideView(
            item.Code,
            item.Title,
            item.Badge,
            Text.Truncate(item.JobCode, 120),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAccessBadgeOverrideView> ToDefaultAccessBadgeOverrideViews(List<DefaultAccessBadgeOverride> items)
    {
        var views = new List<DefaultAccessBadgeOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAccessBadgeOverrideView(item));
        }
        return views;
    }

    public static double SumDefaultAccessBadgeOverrideView(DefaultAccessBadgeOverrideView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedMinimum;
    }
}
