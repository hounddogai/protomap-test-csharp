namespace Acme.Workforce.Engagement.Recognition.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition proposal.</summary>
public sealed record RecognitionProposalView(string CodeText, string Name, string ExpiresAt, string CostText, string Grade, string StatusLabel)
{
    public static RecognitionProposalView ToRecognitionProposalView(RecognitionProposal item)
    {
        return new RecognitionProposalView(
            item.Code,
            item.Name,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionProposalView> ToRecognitionProposalViews(List<RecognitionProposal> items)
    {
        var views = new List<RecognitionProposalView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionProposalView(item));
        }
        return views;
    }

    public static double SumRecognitionProposalView(RecognitionProposalView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedCost;
    }
}
