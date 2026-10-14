namespace Acme.Workforce.Compliance.Audit.Mapping;

using System.Globalization;

/// <summary>How screens and exports show a audit template.</summary>
public sealed record AuditTemplateView(string Code, string Title, string Skill, string Unit, string Approved, string Reviewer, string QuantityText, string StatusLabel)
{
    public static AuditTemplateView ToAuditTemplateView(AuditTemplate item)
    {
        return new AuditTemplateView(
            item.Code,
            item.Title,
            item.Skill,
            item.Unit,
            $"{item.Approved}",
            item.Reviewer,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditTemplateView> ToAuditTemplateViews(List<AuditTemplate> items)
    {
        var views = new List<AuditTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToAuditTemplateView(item));
        }
        return views;
    }

    public static double SumAuditTemplateView(AuditTemplateView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
