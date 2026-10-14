namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant audit.</summary>
public sealed record EquityGrantAuditView(string CodeText, string Title, string Source, string NameText, string Maximum, string Country, string StatusLabel)
{
    public static EquityGrantAuditView ToEquityGrantAuditView(EquityGrantAudit item)
    {
        return new EquityGrantAuditView(
            item.Code,
            item.Title,
            Text.Truncate(item.Source, 80),
            Text.Truncate(item.Name, 80),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantAuditView> ToEquityGrantAuditViews(List<EquityGrantAudit> items)
    {
        var views = new List<EquityGrantAuditView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantAuditView(item));
        }
        return views;
    }

    public static double SumEquityGrantAuditView(EquityGrantAuditView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
