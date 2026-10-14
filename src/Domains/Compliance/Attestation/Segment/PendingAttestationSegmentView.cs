namespace Acme.Workforce.Compliance.Attestation.Segment;

using System.Globalization;

/// <summary>How screens and exports show a pending attestation segment.</summary>
public sealed record PendingAttestationSegmentView(string Code, string Title, string WindowDays, string AmountText, string Pinned, string StatusLabel)
{
    public static PendingAttestationSegmentView ToPendingAttestationSegmentView(PendingAttestationSegment item)
    {
        return new PendingAttestationSegmentView(
            item.Code,
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAttestationSegmentView> ToPendingAttestationSegmentViews(List<PendingAttestationSegment> items)
    {
        var views = new List<PendingAttestationSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAttestationSegmentView(item));
        }
        return views;
    }

    public static double SumPendingAttestationSegmentView(PendingAttestationSegmentView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedAmount;
    }
}
