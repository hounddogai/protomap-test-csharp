namespace Acme.Workforce.Payrollops.Payslip.Budget;

using System.Globalization;

/// <summary>How screens and exports show a default payslip budget.</summary>
public sealed record DefaultPayslipBudgetView(string Code, string Title, string RevisionText, string Version, string StartsAt, string TimezoneText, string Threshold, string StatusLabel)
{
    public static DefaultPayslipBudgetView ToDefaultPayslipBudgetView(DefaultPayslipBudget item)
    {
        return new DefaultPayslipBudgetView(
            item.Code,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPayslipBudgetView> ToDefaultPayslipBudgetViews(List<DefaultPayslipBudget> items)
    {
        var views = new List<DefaultPayslipBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPayslipBudgetView(item));
        }
        return views;
    }

    public static double SumDefaultPayslipBudgetView(DefaultPayslipBudgetView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedVersion + (double)parsedStartsAt;
    }
}
