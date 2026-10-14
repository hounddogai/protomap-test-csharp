namespace Acme.Workforce.Compliance.Audit.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a global audit template.</summary>
public sealed record GlobalAuditTemplateView(string Code, string Label, string UpdatedAt, string JobCode, string Limit, string OwnerIdText, string Source, string BatchSizeText, string Name, string StatusLabel)
{
    public static GlobalAuditTemplateView ToGlobalAuditTemplateView(GlobalAuditTemplate item)
    {
        return new GlobalAuditTemplateView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Source,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAuditTemplateView> ToGlobalAuditTemplateViews(List<GlobalAuditTemplate> items)
    {
        var views = new List<GlobalAuditTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAuditTemplateView(item));
        }
        return views;
    }

    public static double SumGlobalAuditTemplateView(GlobalAuditTemplateView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedLimit + (double)parsedBatchSize;
    }
}
