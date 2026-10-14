namespace Acme.Workforce.Payrollops.Payslip.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default payslip review.</summary>
public sealed record DefaultPayslipReviewView(string Code, string Title, string SalaryBandText, string ContactEmail, string StartsAtText, string Archived, string Minimum, string Revision, string EndsAt, string StatusLabel)
{
    public static DefaultPayslipReviewView ToDefaultPayslipReviewView(DefaultPayslipReview item)
    {
        return new DefaultPayslipReviewView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.SalaryBand,
            Text.Truncate(item.ContactEmail, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPayslipReviewView> ToDefaultPayslipReviewViews(List<DefaultPayslipReview> items)
    {
        var views = new List<DefaultPayslipReviewView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPayslipReviewView(item));
        }
        return views;
    }

    public static double SumDefaultPayslipReviewView(DefaultPayslipReviewView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMinimum + (double)parsedRevision;
    }
}
