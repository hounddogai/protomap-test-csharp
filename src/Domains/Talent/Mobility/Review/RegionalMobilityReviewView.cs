namespace Acme.Workforce.Talent.Mobility.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional mobility review.</summary>
public sealed record RegionalMobilityReviewView(string CodeText, string TitleText, string Retries, string ManagerId, string EmployeeId, string Score, string Channel, string FteText, string EndsAtText, string Visible, string Department, string StatusLabel)
{
    public static RegionalMobilityReviewView ToRegionalMobilityReviewView(RegionalMobilityReview item)
    {
        return new RegionalMobilityReviewView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.EmployeeId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalMobilityReviewView> ToRegionalMobilityReviewViews(List<RegionalMobilityReview> items)
    {
        var views = new List<RegionalMobilityReviewView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalMobilityReviewView(item));
        }
        return views;
    }

    public static double SumRegionalMobilityReviewView(RegionalMobilityReviewView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedScore + parsedFte;
    }
}
