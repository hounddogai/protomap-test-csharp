namespace Acme.Workforce.Engagement.TownHall.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall claim.</summary>
public sealed record TownHallClaimView(string Code, string TitleText, string OwnerIdText, string SummaryText, string Notes, string Ratio, string RateText, string ChecksumText, string Name, string StatusLabel)
{
    public static TownHallClaimView ToTownHallClaimView(TownHallClaim item)
    {
        return new TownHallClaimView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.OwnerId, 40),
            item.Summary,
            Text.Truncate(item.Notes, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            Text.Truncate(item.Name, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallClaimView> ToTownHallClaimViews(List<TownHallClaim> items)
    {
        var views = new List<TownHallClaimView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallClaimView(item));
        }
        return views;
    }

    public static double SumTownHallClaimView(TownHallClaimView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedRate;
    }
}
