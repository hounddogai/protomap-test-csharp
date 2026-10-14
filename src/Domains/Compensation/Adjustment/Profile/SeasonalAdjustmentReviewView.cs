namespace Acme.Workforce.Compensation.Adjustment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal adjustment review.</summary>
public sealed record SeasonalAdjustmentReviewView(string CodeText, string TitleText, string Ceiling, string NotesText, string LabelText, string Revision, string UpdatedAt, string BatchSizeText, string StatusLabel)
{
    public static SeasonalAdjustmentReviewView ToSeasonalAdjustmentReviewView(SeasonalAdjustmentReview item)
    {
        return new SeasonalAdjustmentReviewView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAdjustmentReviewView> ToSeasonalAdjustmentReviewViews(List<SeasonalAdjustmentReview> items)
    {
        var views = new List<SeasonalAdjustmentReviewView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAdjustmentReviewView(item));
        }
        return views;
    }

    public static double SumSeasonalAdjustmentReviewView(SeasonalAdjustmentReviewView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedRevision + (double)parsedUpdatedAt;
    }
}
