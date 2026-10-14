namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model audit.</summary>
public sealed record CostModelAuditView(string CodeText, string Title, string Location, string SourceText, string Region, string CreatedAt, string StatusLabel)
{
    public static CostModelAuditView ToCostModelAuditView(CostModelAudit item)
    {
        return new CostModelAuditView(
            item.Code,
            item.Title,
            Text.Truncate(item.Location, 40),
            item.Source,
            Text.Truncate(item.Region, 120),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelAuditView> ToCostModelAuditViews(List<CostModelAudit> items)
    {
        var views = new List<CostModelAuditView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelAuditView(item));
        }
        return views;
    }

    public static double SumCostModelAuditView(CostModelAuditView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt;
    }
}
