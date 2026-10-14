namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge allocation.</summary>
public sealed record AccessBadgeAllocationView(string Code, string LabelText, string Retries, string AttemptsText, string ContactEmail, string StatusLabel)
{
    public static AccessBadgeAllocationView ToAccessBadgeAllocationView(AccessBadgeAllocation item)
    {
        return new AccessBadgeAllocationView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeAllocationView> ToAccessBadgeAllocationViews(List<AccessBadgeAllocation> items)
    {
        var views = new List<AccessBadgeAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeAllocationView(item));
        }
        return views;
    }

    public static double SumAccessBadgeAllocationView(AccessBadgeAllocationView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedAttempts;
    }
}
