namespace Acme.Workforce.Compliance.Audit.Claim;

using System.Globalization;

/// <summary>How screens and exports show a lenient audit plan.</summary>
public sealed record LenientAuditPlanView(string Code, string LabelText, string Timezone, string CreatedAtText, string CostCenter, string EndsAt, string ExternalRef, string StatusLabel)
{
    public static LenientAuditPlanView ToLenientAuditPlanView(LenientAuditPlan item)
    {
        return new LenientAuditPlanView(
            item.Code,
            item.Label,
            item.Timezone,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientAuditPlanView> ToLenientAuditPlanViews(List<LenientAuditPlan> items)
    {
        var views = new List<LenientAuditPlanView>();
        foreach (var item in items)
        {
            views.Add(ToLenientAuditPlanView(item));
        }
        return views;
    }

    public static double SumLenientAuditPlanView(LenientAuditPlanView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedEndsAt;
    }
}
