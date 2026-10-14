namespace Acme.Workforce.Payrollops.Deduction.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction segment.</summary>
public sealed record DeductionSegmentView(string Code, string Name, string Location, string Rate, string Ratio, string ExternalRef, string Reviewer, string StatusLabel)
{
    public static DeductionSegmentView ToDeductionSegmentView(DeductionSegment item)
    {
        return new DeductionSegmentView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Location, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionSegmentView> ToDeductionSegmentViews(List<DeductionSegment> items)
    {
        var views = new List<DeductionSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionSegmentView(item));
        }
        return views;
    }

    public static double SumDeductionSegmentView(DeductionSegmentView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRate + parsedRatio;
    }
}
