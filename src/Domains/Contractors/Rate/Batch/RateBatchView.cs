namespace Acme.Workforce.Contractors.Rate.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate batch.</summary>
public sealed record RateBatchView(string Code, string Title, string Version, string Segment, string BadgeText, string Revision, string CurrencyText, string FloorValueText, string VisibleText, string StartsAt, string StatusLabel)
{
    public static RateBatchView ToRateBatchView(RateBatch item)
    {
        return new RateBatchView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            Text.Truncate(item.Badge, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateBatchView> ToRateBatchViews(List<RateBatch> items)
    {
        var views = new List<RateBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRateBatchView(item));
        }
        return views;
    }

    public static double SumRateBatchView(RateBatchView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedRevision + parsedFloorValue;
    }
}
