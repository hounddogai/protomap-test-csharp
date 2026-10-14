namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered engagement agreement.</summary>
public sealed record TieredEngagementAgreementView(string Code, string Title, string Attempts, string Approved, string Priority, string StatusLabel)
{
    public static TieredEngagementAgreementView ToTieredEngagementAgreementView(TieredEngagementAgreement item)
    {
        return new TieredEngagementAgreementView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredEngagementAgreementView> ToTieredEngagementAgreementViews(List<TieredEngagementAgreement> items)
    {
        var views = new List<TieredEngagementAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToTieredEngagementAgreementView(item));
        }
        return views;
    }

    public static double SumTieredEngagementAgreementView(TieredEngagementAgreementView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedPriority;
    }
}
