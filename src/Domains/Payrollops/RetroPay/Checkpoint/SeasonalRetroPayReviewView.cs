namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal retro pay review.</summary>
public sealed record SeasonalRetroPayReviewView(string CodeText, string NameText, string SourceText, string Phone, string Pinned, string LeaveDays, string RatingText, string Category, string StatusLabel)
{
    public static SeasonalRetroPayReviewView ToSeasonalRetroPayReviewView(SeasonalRetroPayReview item)
    {
        return new SeasonalRetroPayReviewView(
            item.Code,
            item.Name,
            Text.Truncate(item.Source, 120),
            item.Phone,
            $"{item.Pinned}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalRetroPayReviewView> ToSeasonalRetroPayReviewViews(List<SeasonalRetroPayReview> items)
    {
        var views = new List<SeasonalRetroPayReviewView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalRetroPayReviewView(item));
        }
        return views;
    }

    public static double SumSeasonalRetroPayReviewView(SeasonalRetroPayReviewView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedRating;
    }
}
