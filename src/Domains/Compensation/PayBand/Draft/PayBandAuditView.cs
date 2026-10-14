namespace Acme.Workforce.Compensation.PayBand.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band audit.</summary>
public sealed record PayBandAuditView(string CodeText, string Label, string SummaryText, string Weight, string Description, string ContactEmail, string Fte, string DepartmentText, string Grade, string TitleText, string Retries, string StatusLabel)
{
    public static PayBandAuditView ToPayBandAuditView(PayBandAudit item)
    {
        return new PayBandAuditView(
            item.Code,
            item.Label,
            item.Summary,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.ContactEmail, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Grade,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandAuditView> ToPayBandAuditViews(List<PayBandAudit> items)
    {
        var views = new List<PayBandAuditView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandAuditView(item));
        }
        return views;
    }

    public static double SumPayBandAuditView(PayBandAuditView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedWeight + parsedFte + (double)parsedRetries;
    }
}
