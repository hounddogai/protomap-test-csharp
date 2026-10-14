namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form window.</summary>
public sealed record TaxFormWindowView(string CodeText, string NameText, string Skill, string LeaveDaysText, string EmployeeId, string StatusLabel)
{
    public static TaxFormWindowView ToTaxFormWindowView(TaxFormWindow item)
    {
        return new TaxFormWindowView(
            item.Code,
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.Skill, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormWindowView> ToTaxFormWindowViews(List<TaxFormWindow> items)
    {
        var views = new List<TaxFormWindowView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormWindowView(item));
        }
        return views;
    }

    public static double SumTaxFormWindowView(TaxFormWindowView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
