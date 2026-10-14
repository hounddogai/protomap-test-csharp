namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling recognition proposal.</summary>
public sealed record RollingRecognitionProposalView(string CodeText, string Label, string Quantity, string Limit, string Fte, string CapacityText, string Country, string StatusLabel)
{
    public static RollingRecognitionProposalView ToRollingRecognitionProposalView(RollingRecognitionProposal item)
    {
        return new RollingRecognitionProposalView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRecognitionProposalView> ToRollingRecognitionProposalViews(List<RollingRecognitionProposal> items)
    {
        var views = new List<RollingRecognitionProposalView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRecognitionProposalView(item));
        }
        return views;
    }

    public static double SumRollingRecognitionProposalView(RollingRecognitionProposalView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedLimit + parsedFte;
    }
}
