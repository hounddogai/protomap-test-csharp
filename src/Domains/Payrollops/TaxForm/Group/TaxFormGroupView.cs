namespace Acme.Workforce.Payrollops.TaxForm.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form group.</summary>
public sealed record TaxFormGroupView(string CodeText, string Label, string Minimum, string SequenceText, string Checksum, string Weight, string StatusLabel)
{
    public static TaxFormGroupView ToTaxFormGroupView(TaxFormGroup item)
    {
        return new TaxFormGroupView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormGroupView> ToTaxFormGroupViews(List<TaxFormGroup> items)
    {
        var views = new List<TaxFormGroupView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormGroupView(item));
        }
        return views;
    }

    public static double SumTaxFormGroupView(TaxFormGroupView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedSequence + parsedWeight;
    }
}
