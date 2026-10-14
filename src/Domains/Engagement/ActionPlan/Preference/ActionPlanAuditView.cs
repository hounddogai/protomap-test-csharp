namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan audit.</summary>
public sealed record ActionPlanAuditView(string CodeText, string Title, string Country, string Revision, string Currency, string Channel, string StatusLabel)
{
    public static ActionPlanAuditView ToActionPlanAuditView(ActionPlanAudit item)
    {
        return new ActionPlanAuditView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Country,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanAuditView> ToActionPlanAuditViews(List<ActionPlanAudit> items)
    {
        var views = new List<ActionPlanAuditView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanAuditView(item));
        }
        return views;
    }

    public static double SumActionPlanAuditView(ActionPlanAuditView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
