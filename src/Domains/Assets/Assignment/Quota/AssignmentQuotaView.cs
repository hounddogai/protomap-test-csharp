namespace Acme.Workforce.Assets.Assignment.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment quota.</summary>
public sealed record AssignmentQuotaView(string CodeText, string Label, string Pinned, string Name, string Notes, string Phone, string Retries, string ContactEmail, string Amount, string Currency, string OwnerId, string StatusLabel)
{
    public static AssignmentQuotaView ToAssignmentQuotaView(AssignmentQuota item)
    {
        return new AssignmentQuotaView(
            Text.Truncate(item.Code, 64),
            item.Label,
            $"{item.Pinned}",
            Text.Truncate(item.Name, 64),
            item.Notes,
            item.Phone,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            Text.Truncate(item.OwnerId, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentQuotaView> ToAssignmentQuotaViews(List<AssignmentQuota> items)
    {
        var views = new List<AssignmentQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentQuotaView(item));
        }
        return views;
    }

    public static double SumAssignmentQuotaView(AssignmentQuotaView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedAmount;
    }
}
