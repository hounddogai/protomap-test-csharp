namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation request.</summary>
public sealed record AttestationRequestView(string Code, string Label, string Margin, string Sequence, string BatchSize, string Team, string ExpiresAt, string PriorityText, string OwnerId, string Minimum, string StatusLabel)
{
    public static AttestationRequestView ToAttestationRequestView(AttestationRequest item)
    {
        return new AttestationRequestView(
            item.Code,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationRequestView> ToAttestationRequestViews(List<AttestationRequest> items)
    {
        var views = new List<AttestationRequestView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationRequestView(item));
        }
        return views;
    }

    public static double SumAttestationRequestView(AttestationRequestView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedSequence + (double)parsedBatchSize;
    }
}
