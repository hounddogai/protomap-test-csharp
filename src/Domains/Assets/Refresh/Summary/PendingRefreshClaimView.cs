namespace Acme.Workforce.Assets.Refresh.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending refresh claim.</summary>
public sealed record PendingRefreshClaimView(string CodeText, string Title, string Label, string Grade, string ExternalRef, string Country, string RateText, string StatusLabel)
{
    public static PendingRefreshClaimView ToPendingRefreshClaimView(PendingRefreshClaim item)
    {
        return new PendingRefreshClaimView(
            item.Code,
            item.Title,
            Text.Truncate(item.Label, 40),
            item.Grade,
            Text.Truncate(item.ExternalRef, 40),
            item.Country,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingRefreshClaimView> ToPendingRefreshClaimViews(List<PendingRefreshClaim> items)
    {
        var views = new List<PendingRefreshClaimView>();
        foreach (var item in items)
        {
            views.Add(ToPendingRefreshClaimView(item));
        }
        return views;
    }

    public static double SumPendingRefreshClaimView(PendingRefreshClaimView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
