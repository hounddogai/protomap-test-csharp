namespace Acme.Workforce.Talent.Mobility.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility review.</summary>
public sealed record MobilityReviewView(string Code, string Label, string WeightText, string RateText, string SourceText, string StatusLabel)
{
    public static MobilityReviewView ToMobilityReviewView(MobilityReview item)
    {
        return new MobilityReviewView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityReviewView> ToMobilityReviewViews(List<MobilityReview> items)
    {
        var views = new List<MobilityReviewView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityReviewView(item));
        }
        return views;
    }

    public static double SumMobilityReviewView(MobilityReviewView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedRate;
    }
}
