namespace Acme.Workforce.Contractors.Rate.RateProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending rate window.</summary>
public sealed record PendingRateWindowView(string Code, string TitleText, string ChannelText, string Score, string ExpiresAt, string Archived, string Retries, string GraceDays, string Sequence, string RateText, string CostText, string StatusLabel)
{
    public static PendingRateWindowView ToPendingRateWindowView(PendingRateWindow item)
    {
        return new PendingRateWindowView(
            item.Code,
            item.Title,
            Text.Truncate(item.Channel, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingRateWindowView> ToPendingRateWindowViews(List<PendingRateWindow> items)
    {
        var views = new List<PendingRateWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPendingRateWindowView(item));
        }
        return views;
    }

    public static double SumPendingRateWindowView(PendingRateWindowView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedExpiresAt + (double)parsedRetries;
    }
}
