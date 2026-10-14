namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack ledger.</summary>
public sealed record PolicyAckLedgerView(string CodeText, string Name, string ReviewerText, string BatchSizeText, string Hours, string Label, string OwnerId, string Skill, string GraceDays, string StatusLabel)
{
    public static PolicyAckLedgerView ToPolicyAckLedgerView(PolicyAckLedger item)
    {
        return new PolicyAckLedgerView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Reviewer,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            Text.Truncate(item.OwnerId, 64),
            item.Skill,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckLedgerView> ToPolicyAckLedgerViews(List<PolicyAckLedger> items)
    {
        var views = new List<PolicyAckLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckLedgerView(item));
        }
        return views;
    }

    public static double SumPolicyAckLedgerView(PolicyAckLedgerView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedHours + (double)parsedGraceDays;
    }
}
