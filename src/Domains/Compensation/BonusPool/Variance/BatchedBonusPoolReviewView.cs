namespace Acme.Workforce.Compensation.BonusPool.Variance;

using System.Globalization;

/// <summary>How screens and exports show a batched bonus pool review.</summary>
public sealed record BatchedBonusPoolReviewView(string CodeText, string Title, string StartsAt, string Hours, string Revision, string ManagerIdText, string ExternalRef, string Checksum, string Ceiling, string UpdatedAt, string StatusLabel)
{
    public static BatchedBonusPoolReviewView ToBatchedBonusPoolReviewView(BatchedBonusPoolReview item)
    {
        return new BatchedBonusPoolReviewView(
            item.Code,
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.ExternalRef,
            item.Checksum,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedBonusPoolReviewView> ToBatchedBonusPoolReviewViews(List<BatchedBonusPoolReview> items)
    {
        var views = new List<BatchedBonusPoolReviewView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedBonusPoolReviewView(item));
        }
        return views;
    }

    public static double SumBatchedBonusPoolReviewView(BatchedBonusPoolReviewView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedHours + (double)parsedRevision;
    }
}
