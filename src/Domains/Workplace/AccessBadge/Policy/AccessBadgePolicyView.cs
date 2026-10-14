namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge policy.</summary>
public sealed record AccessBadgePolicyView(string CodeText, string Title, string ExpiresAt, string Phone, string Description, string Segment, string StatusLabel)
{
    public static AccessBadgePolicyView ToAccessBadgePolicyView(AccessBadgePolicy item)
    {
        return new AccessBadgePolicyView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Description,
            Text.Truncate(item.Segment, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgePolicyView> ToAccessBadgePolicyViews(List<AccessBadgePolicy> items)
    {
        var views = new List<AccessBadgePolicyView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgePolicyView(item));
        }
        return views;
    }

    public static double SumAccessBadgePolicyView(AccessBadgePolicyView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
