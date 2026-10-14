namespace Acme.Workforce.Compliance.Exception.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception review.</summary>
public sealed record ExceptionReviewView(string CodeText, string Name, string Discount, string ScoreText, string Locale, string TitleText, string StartsAt, string CreatedAt, string Ratio, string CategoryText, string StatusLabel)
{
    public static ExceptionReviewView ToExceptionReviewView(ExceptionReview item)
    {
        return new ExceptionReviewView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionReviewView> ToExceptionReviewViews(List<ExceptionReview> items)
    {
        var views = new List<ExceptionReviewView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionReviewView(item));
        }
        return views;
    }

    public static double SumExceptionReviewView(ExceptionReviewView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedScore + (double)parsedStartsAt;
    }
}
