namespace Acme.Workforce.Engagement.TownHall.Claim;

using System.Globalization;

/// <summary>How screens and exports show a town hall quota.</summary>
public sealed record TownHallQuotaView(string Code, string TitleText, string Discount, string VersionText, string Skill, string EndsAt, string WindowDaysText, string Team, string Country, string StatusLabel)
{
    public static TownHallQuotaView ToTownHallQuotaView(TownHallQuota item)
    {
        return new TownHallQuotaView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallQuotaView> ToTownHallQuotaViews(List<TownHallQuota> items)
    {
        var views = new List<TownHallQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallQuotaView(item));
        }
        return views;
    }

    public static double SumTownHallQuotaView(TownHallQuotaView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedVersion + (double)parsedEndsAt;
    }
}
