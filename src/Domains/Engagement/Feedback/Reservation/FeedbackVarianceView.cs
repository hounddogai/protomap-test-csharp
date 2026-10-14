namespace Acme.Workforce.Engagement.Feedback.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback variance.</summary>
public sealed record FeedbackVarianceView(string CodeText, string Title, string ApprovedText, string Cost, string Locale, string Region, string SalaryBand, string StatusLabel)
{
    public static FeedbackVarianceView ToFeedbackVarianceView(FeedbackVariance item)
    {
        return new FeedbackVarianceView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Region,
            Text.Truncate(item.SalaryBand, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackVarianceView> ToFeedbackVarianceViews(List<FeedbackVariance> items)
    {
        var views = new List<FeedbackVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackVarianceView(item));
        }
        return views;
    }

    public static double SumFeedbackVarianceView(FeedbackVarianceView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
