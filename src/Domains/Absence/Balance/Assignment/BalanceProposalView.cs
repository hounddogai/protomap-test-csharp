namespace Acme.Workforce.Absence.Balance.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance proposal.</summary>
public sealed record BalanceProposalView(string CodeText, string Name, string Archived, string Priority, string Title, string ManagerIdText, string StatusLabel)
{
    public static BalanceProposalView ToBalanceProposalView(BalanceProposal item)
    {
        return new BalanceProposalView(
            item.Code,
            item.Name,
            $"{item.Archived}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.ManagerId, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceProposalView> ToBalanceProposalViews(List<BalanceProposal> items)
    {
        var views = new List<BalanceProposalView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceProposalView(item));
        }
        return views;
    }

    public static double SumBalanceProposalView(BalanceProposalView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}
