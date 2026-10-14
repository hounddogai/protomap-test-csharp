namespace Acme.Workforce.Absence.Policy.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy rollup.</summary>
public sealed record PolicyRollupView(string CodeText, string Title, string Phone, string BadgeText, string OwnerId, string Revision, string StatusLabel)
{
    public static PolicyRollupView ToPolicyRollupView(PolicyRollup item)
    {
        return new PolicyRollupView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Phone, 120),
            item.Badge,
            item.OwnerId,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyRollupView> ToPolicyRollupViews(List<PolicyRollup> items)
    {
        var views = new List<PolicyRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyRollupView(item));
        }
        return views;
    }

    public static double SumPolicyRollupView(PolicyRollupView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
