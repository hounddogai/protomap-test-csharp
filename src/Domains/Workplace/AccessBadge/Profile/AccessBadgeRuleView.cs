namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge rule.</summary>
public sealed record AccessBadgeRuleView(string CodeText, string LabelText, string Channel, string Phone, string OwnerId, string Score, string StatusLabel)
{
    public static AccessBadgeRuleView ToAccessBadgeRuleView(AccessBadgeRule item)
    {
        return new AccessBadgeRuleView(
            item.Code,
            item.Label,
            Text.Truncate(item.Channel, 40),
            item.Phone,
            item.OwnerId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeRuleView> ToAccessBadgeRuleViews(List<AccessBadgeRule> items)
    {
        var views = new List<AccessBadgeRuleView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeRuleView(item));
        }
        return views;
    }

    public static double SumAccessBadgeRuleView(AccessBadgeRuleView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
