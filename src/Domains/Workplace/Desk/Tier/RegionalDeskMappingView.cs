namespace Acme.Workforce.Workplace.Desk.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional desk mapping.</summary>
public sealed record RegionalDeskMappingView(string Code, string Title, string OwnerIdText, string Rate, string Country, string LimitText, string Channel, string EndsAt, string StatusLabel)
{
    public static RegionalDeskMappingView ToRegionalDeskMappingView(RegionalDeskMapping item)
    {
        return new RegionalDeskMappingView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.OwnerId, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalDeskMappingView> ToRegionalDeskMappingViews(List<RegionalDeskMapping> items)
    {
        var views = new List<RegionalDeskMappingView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalDeskMappingView(item));
        }
        return views;
    }

    public static double SumRegionalDeskMappingView(RegionalDeskMappingView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedLimit + (double)parsedEndsAt;
    }
}
