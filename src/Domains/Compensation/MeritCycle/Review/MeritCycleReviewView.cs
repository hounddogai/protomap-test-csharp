namespace Acme.Workforce.Compensation.MeritCycle.Review;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle review.</summary>
public sealed record MeritCycleReviewView(string CodeText, string Title, string Pinned, string Ratio, string UpdatedAtText, string LabelText, string Approved, string StatusLabel)
{
    public static MeritCycleReviewView ToMeritCycleReviewView(MeritCycleReview item)
    {
        return new MeritCycleReviewView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Label,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleReviewView> ToMeritCycleReviewViews(List<MeritCycleReview> items)
    {
        var views = new List<MeritCycleReviewView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleReviewView(item));
        }
        return views;
    }

    public static double SumMeritCycleReviewView(MeritCycleReviewView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedUpdatedAt;
    }
}
