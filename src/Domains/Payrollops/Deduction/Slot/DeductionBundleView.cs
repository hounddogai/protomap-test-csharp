namespace Acme.Workforce.Payrollops.Deduction.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction bundle.</summary>
public sealed record DeductionBundleView(string Code, string Name, string StartsAtText, string Phone, string NotesText, string Source, string DescriptionText, string StatusLabel)
{
    public static DeductionBundleView ToDeductionBundleView(DeductionBundle item)
    {
        return new DeductionBundleView(
            item.Code,
            item.Name,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 80),
            item.Notes,
            item.Source,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionBundleView> ToDeductionBundleViews(List<DeductionBundle> items)
    {
        var views = new List<DeductionBundleView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionBundleView(item));
        }
        return views;
    }

    public static double SumDeductionBundleView(DeductionBundleView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
