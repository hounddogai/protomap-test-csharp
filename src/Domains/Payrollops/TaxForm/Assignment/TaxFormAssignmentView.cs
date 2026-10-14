namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form assignment.</summary>
public sealed record TaxFormAssignmentView(string Code, string Title, string Phone, string Category, string Priority, string FteText, string ManagerId, string ChecksumText, string Margin, string LocaleText, string SalaryBand, string StatusLabel)
{
    public static TaxFormAssignmentView ToTaxFormAssignmentView(TaxFormAssignment item)
    {
        return new TaxFormAssignmentView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Phone, 120),
            item.Category,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Checksum,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 80),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormAssignmentView> ToTaxFormAssignmentViews(List<TaxFormAssignment> items)
    {
        var views = new List<TaxFormAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormAssignmentView(item));
        }
        return views;
    }

    public static double SumTaxFormAssignmentView(TaxFormAssignmentView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedFte + parsedMargin;
    }
}
