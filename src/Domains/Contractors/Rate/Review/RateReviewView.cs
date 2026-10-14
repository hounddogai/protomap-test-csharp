namespace Acme.Workforce.Contractors.Rate.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate review.</summary>
public sealed record RateReviewView(string CodeText, string Name, string Timezone, string Hours, string Capacity, string StatusLabel)
{
    public static RateReviewView ToRateReviewView(RateReview item)
    {
        return new RateReviewView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Timezone, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateReviewView> ToRateReviewViews(List<RateReview> items)
    {
        var views = new List<RateReviewView>();
        foreach (var item in items)
        {
            views.Add(ToRateReviewView(item));
        }
        return views;
    }

    public static double SumRateReviewView(RateReviewView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedCapacity;
    }
}
