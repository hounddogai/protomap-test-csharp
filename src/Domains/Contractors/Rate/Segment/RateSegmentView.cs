namespace Acme.Workforce.Contractors.Rate.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate segment.</summary>
public sealed record RateSegmentView(string Code, string LabelText, string JobCode, string ContactEmailText, string TenureMonths, string Currency, string EmployeeId, string SalaryBandText, string StatusLabel)
{
    public static RateSegmentView ToRateSegmentView(RateSegment item)
    {
        return new RateSegmentView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.JobCode, 40),
            Text.Truncate(item.ContactEmail, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.EmployeeId,
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateSegmentView> ToRateSegmentViews(List<RateSegment> items)
    {
        var views = new List<RateSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToRateSegmentView(item));
        }
        return views;
    }

    public static double SumRateSegmentView(RateSegmentView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
