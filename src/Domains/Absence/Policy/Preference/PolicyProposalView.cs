namespace Acme.Workforce.Absence.Policy.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy proposal.</summary>
public sealed record PolicyProposalView(string Code, string Name, string Visible, string BatchSize, string OwnerId, string EmployeeId, string Margin, string StatusLabel)
{
    public static PolicyProposalView ToPolicyProposalView(PolicyProposal item)
    {
        return new PolicyProposalView(
            Text.Truncate(item.Code, 40),
            item.Name,
            $"{item.Visible}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.EmployeeId,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyProposalView> ToPolicyProposalViews(List<PolicyProposal> items)
    {
        var views = new List<PolicyProposalView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyProposalView(item));
        }
        return views;
    }

    public static double SumPolicyProposalView(PolicyProposalView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedMargin;
    }
}
