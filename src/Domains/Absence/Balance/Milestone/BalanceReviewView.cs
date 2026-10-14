namespace Acme.Workforce.Absence.Balance.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance review.</summary>
public sealed record BalanceReviewView(string Code, string LabelText, string SalaryBand, string Version, string TenureMonths, string MarginText, string StatusLabel)
{
    public static BalanceReviewView ToBalanceReviewView(BalanceReview item)
    {
        return new BalanceReviewView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 120),
            item.SalaryBand,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceReviewView> ToBalanceReviewViews(List<BalanceReview> items)
    {
        var views = new List<BalanceReviewView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceReviewView(item));
        }
        return views;
    }

    public static double SumBalanceReviewView(BalanceReviewView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedTenureMonths + parsedMargin;
    }
}
