namespace Acme.Workforce.Compensation.PayBand.BandTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending pay band tier.</summary>
public sealed record PendingPayBandTierView(string Code, string Label, string Locale, string GraceDays, string ManagerId, string ChannelText, string Segment, string Ceiling, string ScoreText, string StatusLabel)
{
    public static PendingPayBandTierView ToPendingPayBandTierView(PendingPayBandTier item)
    {
        return new PendingPayBandTierView(
            item.Code,
            item.Label,
            Text.Truncate(item.Locale, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.Channel, 64),
            Text.Truncate(item.Segment, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPayBandTierView> ToPendingPayBandTierViews(List<PendingPayBandTier> items)
    {
        var views = new List<PendingPayBandTierView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPayBandTierView(item));
        }
        return views;
    }

    public static double SumPendingPayBandTierView(PendingPayBandTierView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedCeiling + parsedScore;
    }
}
