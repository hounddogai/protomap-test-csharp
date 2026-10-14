namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted policy ack proposal.</summary>
public sealed record WeightedPolicyAckProposalView(string CodeText, string Title, string Badge, string SequenceText, string MaximumText, string Currency, string WindowDays, string GraceDays, string Rate, string Rating, string StatusLabel)
{
    public static WeightedPolicyAckProposalView ToWeightedPolicyAckProposalView(WeightedPolicyAckProposal item)
    {
        return new WeightedPolicyAckProposalView(
            item.Code,
            item.Title,
            item.Badge,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedPolicyAckProposalView> ToWeightedPolicyAckProposalViews(List<WeightedPolicyAckProposal> items)
    {
        var views = new List<WeightedPolicyAckProposalView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedPolicyAckProposalView(item));
        }
        return views;
    }

    public static double SumWeightedPolicyAckProposalView(WeightedPolicyAckProposalView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedMaximum + (double)parsedWindowDays;
    }
}
