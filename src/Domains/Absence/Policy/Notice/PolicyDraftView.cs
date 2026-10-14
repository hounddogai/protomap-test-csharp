namespace Acme.Workforce.Absence.Policy.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy draft.</summary>
public sealed record PolicyDraftView(string CodeText, string Label, string Priority, string ChannelText, string BudgetText, string Capacity, string Unit, string Visible, string TeamText, string StatusLabel)
{
    public static PolicyDraftView ToPolicyDraftView(PolicyDraft item)
    {
        return new PolicyDraftView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            $"{item.Visible}",
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyDraftView> ToPolicyDraftViews(List<PolicyDraft> items)
    {
        var views = new List<PolicyDraftView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyDraftView(item));
        }
        return views;
    }

    public static double SumPolicyDraftView(PolicyDraftView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedBudget + (double)parsedCapacity;
    }
}
