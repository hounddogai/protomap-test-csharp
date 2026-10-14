namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient tax form assignment.</summary>
public sealed record LenientTaxFormAssignmentView(string Code, string Title, string JobCode, string NotesText, string Minimum, string Ceiling, string ExpiresAt, string Region, string StatusLabel)
{
    public static LenientTaxFormAssignmentView ToLenientTaxFormAssignmentView(LenientTaxFormAssignment item)
    {
        return new LenientTaxFormAssignmentView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.JobCode, 120),
            item.Notes,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientTaxFormAssignmentView> ToLenientTaxFormAssignmentViews(List<LenientTaxFormAssignment> items)
    {
        var views = new List<LenientTaxFormAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToLenientTaxFormAssignmentView(item));
        }
        return views;
    }

    public static double SumLenientTaxFormAssignmentView(LenientTaxFormAssignmentView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedCeiling + (double)parsedExpiresAt;
    }
}
