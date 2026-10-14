namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip review.</summary>
public sealed record PayslipReviewView(string CodeText, string Title, string TenureMonths, string Ceiling, string GraceDays, string Discount, string Skill, string Badge, string Approved, string Quantity, string StatusLabel)
{
    public static PayslipReviewView ToPayslipReviewView(PayslipReview item)
    {
        return new PayslipReviewView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Badge,
            $"{item.Approved}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipReviewView> ToPayslipReviewViews(List<PayslipReview> items)
    {
        var views = new List<PayslipReviewView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipReviewView(item));
        }
        return views;
    }

    public static double SumPayslipReviewView(PayslipReviewView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedCeiling + (double)parsedGraceDays;
    }
}
