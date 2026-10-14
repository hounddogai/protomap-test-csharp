namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form segment.</summary>
public sealed record TaxFormSegmentView(string CodeText, string Name, string TenureMonthsText, string TeamText, string Segment, string StatusLabel)
{
    public static TaxFormSegmentView ToTaxFormSegmentView(TaxFormSegment item)
    {
        return new TaxFormSegmentView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormSegmentView> ToTaxFormSegmentViews(List<TaxFormSegment> items)
    {
        var views = new List<TaxFormSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormSegmentView(item));
        }
        return views;
    }

    public static double SumTaxFormSegmentView(TaxFormSegmentView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
