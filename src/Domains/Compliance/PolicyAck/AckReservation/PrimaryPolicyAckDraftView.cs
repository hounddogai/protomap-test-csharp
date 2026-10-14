namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary policy ack draft.</summary>
public sealed record PrimaryPolicyAckDraftView(string Code, string Title, string Ceiling, string PriorityText, string Sequence, string StatusLabel)
{
    public static PrimaryPolicyAckDraftView ToPrimaryPolicyAckDraftView(PrimaryPolicyAckDraft item)
    {
        return new PrimaryPolicyAckDraftView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPolicyAckDraftView> ToPrimaryPolicyAckDraftViews(List<PrimaryPolicyAckDraft> items)
    {
        var views = new List<PrimaryPolicyAckDraftView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPolicyAckDraftView(item));
        }
        return views;
    }

    public static double SumPrimaryPolicyAckDraftView(PrimaryPolicyAckDraftView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedPriority + (double)parsedSequence;
    }
}
