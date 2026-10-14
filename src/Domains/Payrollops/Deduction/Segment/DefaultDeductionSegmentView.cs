namespace Acme.Workforce.Payrollops.Deduction.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default deduction segment.</summary>
public sealed record DefaultDeductionSegmentView(string Code, string LabelText, string SalaryBand, string SourceText, string OwnerId, string Channel, string Quantity, string StatusLabel)
{
    public static DefaultDeductionSegmentView ToDefaultDeductionSegmentView(DefaultDeductionSegment item)
    {
        return new DefaultDeductionSegmentView(
            item.Code,
            item.Label,
            item.SalaryBand,
            Text.Truncate(item.Source, 40),
            item.OwnerId,
            item.Channel,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultDeductionSegmentView> ToDefaultDeductionSegmentViews(List<DefaultDeductionSegment> items)
    {
        var views = new List<DefaultDeductionSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultDeductionSegmentView(item));
        }
        return views;
    }

    public static double SumDefaultDeductionSegmentView(DefaultDeductionSegmentView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
