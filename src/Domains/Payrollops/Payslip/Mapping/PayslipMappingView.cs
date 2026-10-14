namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip mapping.</summary>
public sealed record PayslipMappingView(string CodeText, string Label, string ExpiresAtText, string ArchivedText, string Title, string ActiveText, string StatusLabel)
{
    public static PayslipMappingView ToPayslipMappingView(PayslipMapping item)
    {
        return new PayslipMappingView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Title,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipMappingView> ToPayslipMappingViews(List<PayslipMapping> items)
    {
        var views = new List<PayslipMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipMappingView(item));
        }
        return views;
    }

    public static double SumPayslipMappingView(PayslipMappingView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
