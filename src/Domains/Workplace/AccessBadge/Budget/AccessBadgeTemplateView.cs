namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge template.</summary>
public sealed record AccessBadgeTemplateView(string Code, string Name, string Badge, string OwnerId, string Label, string ExternalRef, string Department, string Skill, string Amount, string StatusLabel)
{
    public static AccessBadgeTemplateView ToAccessBadgeTemplateView(AccessBadgeTemplate item)
    {
        return new AccessBadgeTemplateView(
            item.Code,
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.Badge, 120),
            Text.Truncate(item.OwnerId, 80),
            Text.Truncate(item.Label, 40),
            item.ExternalRef,
            Text.Truncate(item.Department, 80),
            item.Skill,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeTemplateView> ToAccessBadgeTemplateViews(List<AccessBadgeTemplate> items)
    {
        var views = new List<AccessBadgeTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeTemplateView(item));
        }
        return views;
    }

    public static double SumAccessBadgeTemplateView(AccessBadgeTemplateView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
