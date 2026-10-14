namespace Acme.Workforce.Assets.Refresh.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling refresh preference.</summary>
public sealed record RollingRefreshPreferenceView(string Code, string LabelText, string Currency, string DurationMinutes, string Badge, string CountryText, string ActiveText, string Cost, string ExternalRef, string StatusLabel)
{
    public static RollingRefreshPreferenceView ToRollingRefreshPreferenceView(RollingRefreshPreference item)
    {
        return new RollingRefreshPreferenceView(
            item.Code,
            item.Label,
            item.Currency,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 80),
            item.Country,
            $"{item.Active}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRefreshPreferenceView> ToRollingRefreshPreferenceViews(List<RollingRefreshPreference> items)
    {
        var views = new List<RollingRefreshPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRefreshPreferenceView(item));
        }
        return views;
    }

    public static double SumRollingRefreshPreferenceView(RollingRefreshPreferenceView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedCost;
    }
}
