namespace Acme.Workforce.Talent.Mobility.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive mobility proposal.</summary>
public sealed record AdaptiveMobilityProposalView(string Code, string Name, string BatchSize, string CountryText, string FloorValue, string Currency, string ExternalRef, string Maximum, string StatusLabel)
{
    public static AdaptiveMobilityProposalView ToAdaptiveMobilityProposalView(AdaptiveMobilityProposal item)
    {
        return new AdaptiveMobilityProposalView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            item.ExternalRef,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveMobilityProposalView> ToAdaptiveMobilityProposalViews(List<AdaptiveMobilityProposal> items)
    {
        var views = new List<AdaptiveMobilityProposalView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveMobilityProposalView(item));
        }
        return views;
    }

    public static double SumAdaptiveMobilityProposalView(AdaptiveMobilityProposalView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedFloorValue + parsedMaximum;
    }
}
