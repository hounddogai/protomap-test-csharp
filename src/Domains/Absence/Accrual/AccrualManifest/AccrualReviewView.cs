namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual review.</summary>
public sealed record AccrualReviewView(string Code, string Label, string OwnerIdText, string CurrencyText, string Source, string Hours, string Active, string CostText, string TimezoneText, string DurationMinutes, string StatusLabel)
{
    public static AccrualReviewView ToAccrualReviewView(AccrualReview item)
    {
        return new AccrualReviewView(
            item.Code,
            item.Label,
            item.OwnerId,
            item.Currency,
            item.Source,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 80),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualReviewView> ToAccrualReviewViews(List<AccrualReview> items)
    {
        var views = new List<AccrualReviewView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualReviewView(item));
        }
        return views;
    }

    public static double SumAccrualReviewView(AccrualReviewView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedHours + parsedCost + (double)parsedDurationMinutes;
    }
}
