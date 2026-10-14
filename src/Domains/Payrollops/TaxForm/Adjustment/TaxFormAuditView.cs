namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form audit.</summary>
public sealed record TaxFormAuditView(string Code, string TitleText, string Revision, string LocationText, string MarginText, string Rate, string Grade, string OwnerId, string ContactEmailText, string LocaleText, string Ceiling, string StatusLabel)
{
    public static TaxFormAuditView ToTaxFormAuditView(TaxFormAudit item)
    {
        return new TaxFormAuditView(
            item.Code,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            item.OwnerId,
            item.ContactEmail,
            item.Locale,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormAuditView> ToTaxFormAuditViews(List<TaxFormAudit> items)
    {
        var views = new List<TaxFormAuditView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormAuditView(item));
        }
        return views;
    }

    public static double SumTaxFormAuditView(TaxFormAuditView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedMargin + parsedRate;
    }
}
