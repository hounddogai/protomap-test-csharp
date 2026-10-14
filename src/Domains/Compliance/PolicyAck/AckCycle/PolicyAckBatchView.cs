namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using System.Globalization;

/// <summary>How screens and exports show a policy ack batch.</summary>
public sealed record PolicyAckBatchView(string CodeText, string NameText, string RetriesText, string EmployeeIdText, string ContactEmail, string CreatedAtText, string BudgetText, string Ratio, string RevisionText, string Visible, string EndsAt, string StatusLabel)
{
    public static PolicyAckBatchView ToPolicyAckBatchView(PolicyAckBatch item)
    {
        return new PolicyAckBatchView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.ContactEmail,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckBatchView> ToPolicyAckBatchViews(List<PolicyAckBatch> items)
    {
        var views = new List<PolicyAckBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckBatchView(item));
        }
        return views;
    }

    public static double SumPolicyAckBatchView(PolicyAckBatchView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedCreatedAt + parsedBudget;
    }
}
