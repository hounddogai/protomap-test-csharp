namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack entry.</summary>
public sealed record PolicyAckEntryView(string CodeText, string Label, string Source, string Badge, string Unit, string CountryText, string BatchSizeText, string JobCodeText, string SalaryBandText, string CostCenter, string Attempts, string StatusLabel)
{
    public static PolicyAckEntryView ToPolicyAckEntryView(PolicyAckEntry item)
    {
        return new PolicyAckEntryView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Source, 120),
            item.Badge,
            item.Unit,
            item.Country,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 40),
            item.SalaryBand,
            Text.Truncate(item.CostCenter, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckEntryView> ToPolicyAckEntryViews(List<PolicyAckEntry> items)
    {
        var views = new List<PolicyAckEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckEntryView(item));
        }
        return views;
    }

    public static double SumPolicyAckEntryView(PolicyAckEntryView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedAttempts;
    }
}
