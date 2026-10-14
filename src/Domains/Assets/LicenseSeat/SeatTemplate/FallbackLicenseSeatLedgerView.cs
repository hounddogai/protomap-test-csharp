namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback license seat ledger.</summary>
public sealed record FallbackLicenseSeatLedgerView(string Code, string Title, string Sequence, string Approved, string VersionText, string Archived, string SegmentText, string ExpiresAt, string Checksum, string ChannelText, string StatusLabel)
{
    public static FallbackLicenseSeatLedgerView ToFallbackLicenseSeatLedgerView(FallbackLicenseSeatLedger item)
    {
        return new FallbackLicenseSeatLedgerView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Segment,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackLicenseSeatLedgerView> ToFallbackLicenseSeatLedgerViews(List<FallbackLicenseSeatLedger> items)
    {
        var views = new List<FallbackLicenseSeatLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackLicenseSeatLedgerView(item));
        }
        return views;
    }

    public static double SumFallbackLicenseSeatLedgerView(FallbackLicenseSeatLedgerView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedVersion + (double)parsedExpiresAt;
    }
}
