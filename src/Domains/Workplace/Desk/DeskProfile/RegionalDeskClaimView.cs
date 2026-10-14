namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional desk claim.</summary>
public sealed record RegionalDeskClaimView(string Code, string Name, string Team, string AttemptsText, string Notes, string ExpiresAt, string Pinned, string Minimum, string BatchSizeText, string StatusLabel)
{
    public static RegionalDeskClaimView ToRegionalDeskClaimView(RegionalDeskClaim item)
    {
        return new RegionalDeskClaimView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Team,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalDeskClaimView> ToRegionalDeskClaimViews(List<RegionalDeskClaim> items)
    {
        var views = new List<RegionalDeskClaimView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalDeskClaimView(item));
        }
        return views;
    }

    public static double SumRegionalDeskClaimView(RegionalDeskClaimView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedExpiresAt + parsedMinimum;
    }
}
