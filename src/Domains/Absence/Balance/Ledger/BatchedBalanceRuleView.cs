namespace Acme.Workforce.Absence.Balance.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched balance rule.</summary>
public sealed record BatchedBalanceRuleView(string Code, string Name, string SkillText, string Revision, string CostCenterText, string StatusLabel)
{
    public static BatchedBalanceRuleView ToBatchedBalanceRuleView(BatchedBalanceRule item)
    {
        return new BatchedBalanceRuleView(
            item.Code,
            item.Name,
            Text.Truncate(item.Skill, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedBalanceRuleView> ToBatchedBalanceRuleViews(List<BatchedBalanceRule> items)
    {
        var views = new List<BatchedBalanceRuleView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedBalanceRuleView(item));
        }
        return views;
    }

    public static double SumBatchedBalanceRuleView(BatchedBalanceRuleView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
