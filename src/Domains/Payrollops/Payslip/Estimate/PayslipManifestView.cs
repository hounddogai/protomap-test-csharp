namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip manifest.</summary>
public sealed record PayslipManifestView(string CodeText, string Title, string Score, string Source, string SummaryText, string Hours, string TeamText, string StartsAtText, string UpdatedAt, string StatusLabel)
{
    public static PayslipManifestView ToPayslipManifestView(PayslipManifest item)
    {
        return new PayslipManifestView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Summary,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipManifestView> ToPayslipManifestViews(List<PayslipManifest> items)
    {
        var views = new List<PayslipManifestView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipManifestView(item));
        }
        return views;
    }

    public static double SumPayslipManifestView(PayslipManifestView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedScore + parsedHours + (double)parsedStartsAt;
    }
}
