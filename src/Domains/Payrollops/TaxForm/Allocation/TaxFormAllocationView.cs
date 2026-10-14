namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form allocation.</summary>
public sealed record TaxFormAllocationView(string CodeText, string Label, string Ceiling, string Active, string CreatedAt, string GraceDaysText, string MaximumText, string LeaveDays, string RevisionText, string JobCode, string StatusLabel)
{
    public static TaxFormAllocationView ToTaxFormAllocationView(TaxFormAllocation item)
    {
        return new TaxFormAllocationView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormAllocationView> ToTaxFormAllocationViews(List<TaxFormAllocation> items)
    {
        var views = new List<TaxFormAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormAllocationView(item));
        }
        return views;
    }

    public static double SumTaxFormAllocationView(TaxFormAllocationView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedCreatedAt + (double)parsedGraceDays;
    }
}
