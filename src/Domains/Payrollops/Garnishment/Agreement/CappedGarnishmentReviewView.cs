namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a capped garnishment review.</summary>
public sealed record CappedGarnishmentReviewView(string CodeText, string Title, string Unit, string Quantity, string PinnedText, string Attempts, string FloorValueText, string ExpiresAt, string CreatedAt, string StatusLabel)
{
    public static CappedGarnishmentReviewView ToCappedGarnishmentReviewView(CappedGarnishmentReview item)
    {
        return new CappedGarnishmentReviewView(
            item.Code,
            item.Title,
            item.Unit,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedGarnishmentReviewView> ToCappedGarnishmentReviewViews(List<CappedGarnishmentReview> items)
    {
        var views = new List<CappedGarnishmentReviewView>();
        foreach (var item in items)
        {
            views.Add(ToCappedGarnishmentReviewView(item));
        }
        return views;
    }

    public static double SumCappedGarnishmentReviewView(CappedGarnishmentReviewView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedAttempts + parsedFloorValue;
    }
}
