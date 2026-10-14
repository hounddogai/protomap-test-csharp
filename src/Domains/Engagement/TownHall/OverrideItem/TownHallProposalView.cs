namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a town hall proposal.</summary>
public sealed record TownHallProposalView(string CodeText, string Name, string CreatedAtText, string Skill, string SequenceText, string Fte, string Version, string StatusLabel)
{
    public static TownHallProposalView ToTownHallProposalView(TownHallProposal item)
    {
        return new TownHallProposalView(
            item.Code,
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallProposalView> ToTownHallProposalViews(List<TownHallProposal> items)
    {
        var views = new List<TownHallProposalView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallProposalView(item));
        }
        return views;
    }

    public static double SumTownHallProposalView(TownHallProposalView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedSequence + parsedFte;
    }
}
