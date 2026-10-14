namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped visitor batch.</summary>
public sealed record CappedVisitorBatchView(string Code, string Title, string ExternalRef, string Active, string PhoneText, string Category, string StartsAtText, string UpdatedAtText, string LeaveDays, string StatusLabel)
{
    public static CappedVisitorBatchView ToCappedVisitorBatchView(CappedVisitorBatch item)
    {
        return new CappedVisitorBatchView(
            item.Code,
            item.Title,
            item.ExternalRef,
            $"{item.Active}",
            Text.Truncate(item.Phone, 120),
            Text.Truncate(item.Category, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedVisitorBatchView> ToCappedVisitorBatchViews(List<CappedVisitorBatch> items)
    {
        var views = new List<CappedVisitorBatchView>();
        foreach (var item in items)
        {
            views.Add(ToCappedVisitorBatchView(item));
        }
        return views;
    }

    public static double SumCappedVisitorBatchView(CappedVisitorBatchView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedUpdatedAt + (double)parsedLeaveDays;
    }
}
