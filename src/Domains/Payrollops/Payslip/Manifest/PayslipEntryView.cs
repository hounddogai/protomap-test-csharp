namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip entry.</summary>
public sealed record PayslipEntryView(string Code, string Title, string JobCodeText, string Active, string Location, string Score, string Ceiling, string Retries, string Timezone, string Label, string StatusLabel)
{
    public static PayslipEntryView ToPayslipEntryView(PayslipEntry item)
    {
        return new PayslipEntryView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.JobCode,
            $"{item.Active}",
            item.Location,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipEntryView> ToPayslipEntryViews(List<PayslipEntry> items)
    {
        var views = new List<PayslipEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipEntryView(item));
        }
        return views;
    }

    public static double SumPayslipEntryView(PayslipEntryView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedScore + parsedCeiling + (double)parsedRetries;
    }
}
