namespace Acme.Workforce.Payrollops.Payslip.Group;

using System.Globalization;

/// <summary>How screens and exports show a payslip segment.</summary>
public sealed record PayslipSegmentView(string Code, string Title, string ContactEmail, string UpdatedAt, string Score, string Active, string Region, string TenureMonths, string StatusLabel)
{
    public static PayslipSegmentView ToPayslipSegmentView(PayslipSegment item)
    {
        return new PayslipSegmentView(
            item.Code,
            item.Title,
            item.ContactEmail,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Region,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipSegmentView> ToPayslipSegmentViews(List<PayslipSegment> items)
    {
        var views = new List<PayslipSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipSegmentView(item));
        }
        return views;
    }

    public static double SumPayslipSegmentView(PayslipSegmentView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedScore + (double)parsedTenureMonths;
    }
}
