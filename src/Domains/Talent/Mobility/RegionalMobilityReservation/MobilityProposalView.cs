namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility proposal.</summary>
public sealed record MobilityProposalView(string Code, string Name, string ScoreText, string Title, string Ratio, string Source, string StatusLabel)
{
    public static MobilityProposalView ToMobilityProposalView(MobilityProposal item)
    {
        return new MobilityProposalView(
            item.Code,
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityProposalView> ToMobilityProposalViews(List<MobilityProposal> items)
    {
        var views = new List<MobilityProposalView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityProposalView(item));
        }
        return views;
    }

    public static double SumMobilityProposalView(MobilityProposalView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedScore + parsedRatio;
    }
}
