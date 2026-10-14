namespace Acme.Workforce.Assets.Refresh.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional refresh request.</summary>
public sealed record RegionalRefreshRequestView(string CodeText, string LabelText, string ExpiresAt, string Pinned, string WindowDays, string CreatedAt, string ActiveText, string StatusLabel)
{
    public static RegionalRefreshRequestView ToRegionalRefreshRequestView(RegionalRefreshRequest item)
    {
        return new RegionalRefreshRequestView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalRefreshRequestView> ToRegionalRefreshRequestViews(List<RegionalRefreshRequest> items)
    {
        var views = new List<RegionalRefreshRequestView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalRefreshRequestView(item));
        }
        return views;
    }

    public static double SumRegionalRefreshRequestView(RegionalRefreshRequestView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedWindowDays + (double)parsedCreatedAt;
    }
}
